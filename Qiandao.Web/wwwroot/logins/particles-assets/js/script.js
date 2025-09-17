function _defineProperty(obj, key, value) {if (key in obj) {Object.defineProperty(obj, key, { value: value, enumerable: true, configurable: true, writable: true });} else {obj[key] = value;}return obj;}
/*////////////////////////////////////////////
                                                                                                                                                                                                           ----	Draws a bunch of triangular particles and moves them around canvas
                                                                                                                                                                                                           ///////////////////////////////////////////*/

const Particles = function (canvas, amount, size, speed, idleColor, activeColor, mouseEffectRadius, mouseMoveObject) {

  var Tween = window.TweenMax;

  const $p = function () {};

  /*////////////////////////////////////////////
                             ----	Variables
                             ///////////////////////////////////////////*/

  $p.canvas = canvas;
  $p.context = $p.canvas.getContext('2d');

  $p.settings = {

    amount: amount || 200,
    size: size || 7,
    speed: speed || 0.3,
    colors: { idle: idleColor, active: activeColor } };



  $p.mouse = {

    object: mouseMoveObject || $p.canvas,
    position: { x: 0, y: 0 },
    radius: mouseEffectRadius,
    physics: {

      speed: 0,
      accel: 0,
      angle: 0 },


    timestamp: Date.now() };



  $p.particles = [];
  $p.particlesSectored = [];
  $p.particlesInMouseArea = [];

  $p.paused = true;
  $p.firstInit = true;

  /*////////////////////////////////////////////
                       ----	Functions
                       ///////////////////////////////////////////*/

  // Calculate vector length
  $p.getVectorLength = function (vector) {

    return Math.sqrt(Math.pow(vector.x, 2) + Math.pow(vector.y, 2));

  };

  // Normalize vector ( convert it to 0 to 1 )
  $p.normalizeVector = function (vector) {

    var normalizedVector = { x: 0, y: 0 };

    var vectorLength = $p.getVectorLength(vector);

    normalizedVector.x = vector.x / vectorLength;
    normalizedVector.y = vector.y / vectorLength;

    return normalizedVector;

  };

  // Get vector dot
  $p.getDot = function (vector, point) {

    return Math.abs(vector.x * point.x + vector.y * point.y);

  };

  // Particle constructor
  const Particle = function (id, x, y, size, iColor, aColor) {

    var $particle = function () {};

    /*////////////////////////////////////////////
                                    ----	Variables
                                    ///////////////////////////////////////////*/
    $particle.id = id || false;
    $particle.sector = { x: 0, y: 0 };
    $particle.points = {

      a: { x: 0, y: 0 },
      b: { x: 0, y: 0 },
      c: { x: 0, y: 0 } };


    $particle.startingCoordinates = { x: x, y: y };

    $particle.accel = 1;
    $particle.accelDecayStep = 0.04;

    $particle.hasCollided = false;

    $particle.movingVectorAngle = Math.PI * 2 / (Math.random() * 2 + 1) * (Math.random() * 2 - 1);
    $particle.movement = { x: 0, y: 0 };

    $particle.opacity = Math.random();
    $particle.opacityStep = $p.settings.speed * Math.random() * 10 / 1000;
    $particle.opacityDirection = 1;

    $particle.initColor = iColor;
    $particle.color = iColor;

    $particle.isInMouseRadius = false;
    $particle.connectedTo = [];

    $particle.circleRadius = 0;
    $particle.circleOpacity = 0;
    $particle.circleColor = false;

    $particle.connections = 0;

    /*////////////////////////////////////////////
                               ----	Functions
                               ///////////////////////////////////////////*/

    // Detect collision between two particles ( probably not the correct way to do this, but for this specific purpose, works just fine :)  )
    $particle.detectCollision = $candidate => {

      // If collision was detected in past 250ms, return
      if ($particle.hasCollided) return;

      // Flag: collision detected
      let collisionDetected = true;

      // Array for axises to project points to
      let axisArray = [

      $p.normalizeVector({

        x: -($particle.points.b.y - $particle.points.a.y),
        y: $particle.points.b.x - $particle.points.a.x }),



      $p.normalizeVector({

        x: -($particle.points.c.y - $particle.points.b.y),
        y: $particle.points.c.x - $particle.points.b.x }),



      $p.normalizeVector({

        x: -($particle.points.a.y - $particle.points.c.y),
        y: $particle.points.a.x - $particle.points.c.x })];





      let smallestIntersectionLength = 100000;
      let escapeVector;

      // The loop
      for (let a in axisArray) {

        // Current axis
        let axis = axisArray[a];

        let particleDots = [];
        let candidateDots = [];

        // This particle points loop
        for (let p in $particle.points) {

          // Current particle point
          let currentPoint = $particle.points[p];

          // Projected to axis dot
          particleDots[p] = $p.getDot(axis, currentPoint);

          // Candidate point
          let candidatePoint = $candidate.points[p];

          // Projected to axis dot
          candidateDots[p] = $p.getDot(axis, candidatePoint);

        }

        // Check if there's a space somewhere, that would mean particles do not collide
        if (


        a == 1 && particleDots['c'] < candidateDots['a'] ||
        a == 0 && particleDots['a'] > candidateDots['c'] ||
        a == 2 && (particleDots['b'] < candidateDots['c'] || particleDots['c'] > candidateDots['b']))

        {

          collisionDetected = false;

          break;

        };

        if (a == 1 && particleDots['c'] - candidateDots['a'] < smallestIntersectionLength) {

          escapeVector = axis;

          smallestIntersectionLength = particleDots['c'] - candidateDots['a'];

        } else
        if (a == 0 && candidateDots['c'] - particleDots['a'] < smallestIntersectionLength) {

          escapeVector = axis;

          smallestIntersectionLength = candidateDots['c'] - particleDots['a'];

        } else
        if (a == 2 && (particleDots['b'] - candidateDots['c'] < smallestIntersectionLength || candidateDots['b'] - particleDots['c'] < smallestIntersectionLength)) {

          escapeVector = axis;

          smallestIntersectionLength = Math.min(particleDots['b'] - candidateDots['c'], candidateDots['b'] - particleDots['c']);

        } else
        {escapeVector = axis;}


      }

      // If collision detected, flip moving vectors of particles
      if (collisionDetected) {

        $particle.movingVectorAngle -= Math.atan2(escapeVector.y, escapeVector.x);
        if ($particle.movingVectorAngle > 0 && $candidate.movingVectorAngle > 0 || $particle.movingVectorAngle < 0 && $candidate.movingVectorAngle < 0)
        $candidate.movingVectorAngle -= Math.atan2(escapeVector.y, escapeVector.x);else
        $candidate.movingVectorAngle -= Math.PI - Math.atan2(escapeVector.y, escapeVector.x);


        $particle.accel = 5;
        $candidate.accel = 5;

        $particle.hasCollided = true;
        $candidate.hasCollided = true;

        // Set a 250ms timeout to let particles escape each other
        var t = setTimeout(function () {

          $particle.hasCollided = false;
          $candidate.hasCollided = false;

        }, 3500);

      }

      return collisionDetected;

    };

    // Set new coordinate for particle points
    $particle.createPoints = () => {

      // Calculate amount of movement
      $particle.movement.x = $particle.movement.x + Math.cos($particle.movingVectorAngle) * ($p.settings.speed * (Math.random() + 1)) * $particle.accel;
      $particle.movement.y = $particle.movement.y + Math.sin($particle.movingVectorAngle) * ($p.settings.speed * (Math.random() + 1)) * $particle.accel;

      // Set point A
      $particle.points.a.x = $particle.startingCoordinates.x + $particle.movement.x;
      $particle.points.a.y = $particle.startingCoordinates.y + $particle.movement.y;

      // A point hit any of the walls, reflect the movement
      if ($particle.points.a.y <= 0 || $particle.points.a.y >= $p.canvas.height) $particle.movingVectorAngle = -$particle.movingVectorAngle;
      if ($particle.points.a.x <= 0 || $particle.points.a.x >= $p.canvas.width) $particle.movingVectorAngle = Math.PI - $particle.movingVectorAngle;

      // Set angles to other points
      let angleToB = Math.PI / 3;
      let angleToC = -Math.PI / 3;

      // Set point B
      $particle.points.b.x = $particle.points.a.x + Math.cos(angleToB) * size;
      $particle.points.b.y = $particle.points.a.y + Math.sin(angleToB) * size;

      // Set point C
      $particle.points.c.x = $particle.points.b.x + Math.cos(angleToC) * size;
      $particle.points.c.y = $particle.points.b.y + Math.sin(angleToC) * size;

      // Assign location sector for them
      $particle.sector = {

        x: $particle.points.b.x - $particle.points.b.x % 100,
        y: $particle.points.b.y - $particle.points.b.y % 100 };



    };

    // Check, if current particle is inside mouse area
    $particle.checkIfInMouseRadius = () => {

      if (Math.pow($particle.points.a.x - $p.mouse.position.x, 2) + Math.pow($particle.points.a.y - $p.mouse.position.y, 2) < Math.pow($p.mouse.radius, 2)) {

        $particle.isInMouseRadius = true;
        $p.particlesInMouseArea.push($particle);

      } else
      $particle.isInMouseRadius = false;

    };

    // Change particle color
    $particle.changeColor = () => {

      // If it's not in mouse area, set it do default
      if (!$particle.isInMouseRadius) $particle.color = $particle.initColor;else
      {

        // Get vector from mouse point to particle
        let vector = {

          x: $particle.points.a.x - $p.mouse.position.x,
          y: $particle.points.a.y - $p.mouse.position.y };



        // Get the distance length ( that's the color transition step );
        let vectorLength = $p.mouse.radius - Math.floor(Math.sqrt(vector.x * vector.x + vector.y * vector.y));

        // Calculate step amount for colors
        let colorDifferenceStep = {

          r: (aColor.r - $particle.initColor.r) / $p.mouse.radius,
          g: (aColor.g - $particle.initColor.g) / $p.mouse.radius,
          b: (aColor.b - $particle.initColor.b) / $p.mouse.radius };



        // Adjust color accordingly
        $particle.color = {

          r: Math.round($particle.initColor.r + colorDifferenceStep.r * vectorLength),
          g: Math.round($particle.initColor.g + colorDifferenceStep.g * vectorLength),
          b: Math.round($particle.initColor.b + colorDifferenceStep.b * vectorLength) };



      }

    };

    // Change movement velocity of particle if in mouse area
    $particle.mindTheMouse = function () {

      if (!this.isInMouseRadius) {

        if ($particle.accel > 1) $particle.accel -= $particle.accelDecayStep;
        if ($particle.accel < 1) $particle.accel = 1;

        return;

      } else if ($p.mouse.physics.speed > 5) $particle.accel = $p.mouse.physics.speed;

    };

    // Get center point of a particle
    $particle.getCenterPoint = function ($p) {

      let ACMid = { x: $p.points.a.x + ($p.points.c.x - $p.points.a.x) / 2, y: $p.points.a.y };

      return { x: $p.points.b.x + 2 / 3 * (ACMid.x - $p.points.b.x), y: $p.points.b.y + 2 / 3 * (ACMid.y - $p.points.b.y) };

    };

    // Draw line to another particle
    $particle.connectToParticle = $other => {

      if ($particle.opacity < 0.2 || $other.opacity < 0.2) return;

      // Get center points for particles
      let thisParticleCenter = $particle.getCenterPoint($particle);
      let otherParticleCenter = $particle.getCenterPoint($other);

      // Get vector length between particles
      let vecLength = $p.getVectorLength({ x: thisParticleCenter.x - otherParticleCenter.x, y: thisParticleCenter.y - otherParticleCenter.y });

      // Get conenction points for this vector
      let angleToThis = Math.atan2(thisParticleCenter.y - otherParticleCenter.y, thisParticleCenter.x - otherParticleCenter.x);
      let thisPoints = {

        x: otherParticleCenter.x + Math.cos(angleToThis) * (vecLength - $particle.circleRadius - 1),
        y: otherParticleCenter.y + Math.sin(angleToThis) * (vecLength - $particle.circleRadius - 1) };



      // Get connection points for other vector
      let angleToOther = Math.atan2(otherParticleCenter.y - thisParticleCenter.y, otherParticleCenter.x - thisParticleCenter.x);
      let otherPoints = {

        x: thisPoints.x + Math.cos(angleToOther) * (vecLength - $particle.circleRadius - $other.circleRadius - 2),
        y: thisPoints.y + Math.sin(angleToOther) * (vecLength - $particle.circleRadius - $other.circleRadius - 2) };



      // Draw the line
      $p.context.beginPath();

      $p.context.moveTo(thisPoints.x, thisPoints.y);
      $p.context.lineTo(otherPoints.x, otherPoints.y);

      $p.context.strokeStyle = $particle.circleColor;
      $p.context.stroke();

      $p.context.closePath();

      // Store the conntection
      $particle.connectedTo.push($other.id);

      $particle.connections += 1;

    };

    // Draw line to closest point
    $particle.drawLnToClosest = drawFrom => {

      // Closest point coordinates
      let closest = { x: 0, y: 0 };

      // Lengths of vectors to each point
      let vec_a_len = $p.getVectorLength({ x: drawFrom.x - $particle.points.a.x, y: drawFrom.y - $particle.points.a.y });
      let vec_b_len = $p.getVectorLength({ x: drawFrom.x - $particle.points.b.x, y: drawFrom.y - $particle.points.b.y });
      let vec_c_len = $p.getVectorLength({ x: drawFrom.x - $particle.points.c.x, y: drawFrom.y - $particle.points.c.y });

      // Smallest vector	
      let smallestVec;

      if (vec_a_len < vec_b_len && vec_a_len < vec_c_len) {

        closest = $particle.points.a;
        smallestVec = vec_a_len;

      } else if (vec_b_len < vec_a_len && vec_b_len < vec_c_len) {

        closest = $particle.points.b;
        smallestVec = vec_b_len;

      } else {

        closest = $particle.points.c;
        smallestVec = vec_c_len;

      }

      // Change opacity accordingly to distance
      $particle.circleOpacity = 30 / smallestVec;

      // Set particle color
      $particle.circleColor = "rgba(" + $particle.color.r + "," + $particle.color.g + "," + $particle.color.b + "," + $particle.circleOpacity + ")";

      // Put a circle around the particle
      let midBetweenAC = { x: $particle.points.a.x + ($particle.points.c.x - $particle.points.a.x) / 2, y: $particle.points.a.y };
      let particleCenterPoint = {

        x: $particle.points.b.x + 2 / 3 * (midBetweenAC.x - $particle.points.b.x),
        y: $particle.points.b.y + 2 / 3 * (midBetweenAC.y - $particle.points.b.y) };


      $particle.circleRadius = $p.getVectorLength({ x: particleCenterPoint.x - $particle.points.a.x, y: particleCenterPoint.y - $particle.points.a.y });

      // Adjust the 'closest' to be on the circle
      let lengthFromClosestToCenter = $p.getVectorLength({ x: closest.x - particleCenterPoint.x, y: closest.y - particleCenterPoint.y });
      let segmentToRemove = $particle.circleRadius * 2 - lengthFromClosestToCenter;
      let newSmallestVec = smallestVec - segmentToRemove;
      let closestVector = { x: particleCenterPoint.x - drawFrom.x, y: particleCenterPoint.y - drawFrom.y };
      let closestVectorAngle = Math.atan2(closestVector.y, closestVector.x);
      let newClosest = {

        x: drawFrom.x + Math.cos(closestVectorAngle) * newSmallestVec,
        y: drawFrom.y + Math.sin(closestVectorAngle) * newSmallestVec };



      // Draw the line
      $p.context.beginPath();

      $p.context.arc(particleCenterPoint.x, particleCenterPoint.y, $particle.circleRadius * 2, 0, 2 * Math.PI);

      $p.context.strokeStyle = $particle.circleColor;
      $p.context.stroke();

      $p.context.closePath();
      $p.context.beginPath();

      $p.context.arc(particleCenterPoint.x, particleCenterPoint.y, $particle.circleRadius * 6, 0, 2 * Math.PI);

      $p.context.moveTo(drawFrom.x, drawFrom.y);
      $p.context.lineTo(newClosest.x, newClosest.y);



      $p.context.strokeStyle = $particle.circleColor;
      $p.context.stroke();

      $p.context.closePath();

    };

    // Draw the triangle
    $particle.drawTr = () => {

      // Create particle points
      $particle.createPoints();

      // Check if particle in mouse area
      $particle.checkIfInMouseRadius();

      // Set color accordingly
      $particle.changeColor();

      // Set correct velocity
      //$particle.mindTheMouse();

      // Draw the particle
      $p.context.beginPath();

      $p.context.moveTo($particle.points.a.x, $particle.points.a.y);
      $p.context.lineTo($particle.points.b.x, $particle.points.b.y);
      $p.context.lineTo($particle.points.c.x, $particle.points.c.y);

      $particle.opacity = $particle.opacity + $particle.opacityStep * $particle.opacityDirection;

      if ($particle.opacity <= 0) $particle.opacityDirection = 1;
      if ($particle.opacity >= 1) $particle.opacityDirection = -1;

      $p.context.fillStyle = "rgba(" + $particle.color.r + "," + $particle.color.g + "," + $particle.color.b + "," + $particle.opacity + ")";
      $p.context.fill();

      $p.context.closePath();

      if ($particle.isInMouseRadius && $particle.opacity > 0.2) {

        $particle.drawLnToClosest($p.mouse.position);

      }

    };

    return $particle;

  };

  // Create particles
  $p.createParticles = () => {

    // Position particles +- equally 
    let gapX = $p.canvas.width / $p.settings.amount;
    let gapY = $p.canvas.height / $p.settings.amount;

    // The loop
    for (let i = 0; i <= $p.settings.amount; i++) {

      let size = Math.random() * 4 + $p.settings.size;

      let x = gapX * i;
      let y = gapY * Math.random() * $p.settings.amount + 1;

      $p.particles.push(new Particle(i, x, y, size, $p.settings.colors.idle, $p.settings.colors.active));

    }

  };

  // Drawing function
  $p.drawParticles = () => {

    // Clear the canvas
    $p.context.clearRect(0, 0, $p.canvas.width, $p.canvas.height);

    // Clear sector data
    $p.particlesSectored = [];

    // Clear particles in mouse area
    $p.particlesInMouseArea = [];

    // Loop through all particles
    for (let p in $p.particles) {

      let $particle = $p.particles[p];

      // Reset all connection data;
      $particle.connectedTo = [];

      $particle.connections = 0;

      // Move particle to a sector
      if (!$p.particlesSectored[$particle.sector.x]) $p.particlesSectored[$particle.sector.x] = [];
      if (!$p.particlesSectored[$particle.sector.x][$particle.sector.y]) $p.particlesSectored[$particle.sector.x][$particle.sector.y] = [];

      $p.particlesSectored[$particle.sector.x][$particle.sector.y].push($particle);

      // Draw the particle
      $particle.drawTr();

    };

    // Detect collision between two particles
    for (let sh in $p.particlesSectored) {
      for (let sv in $p.particlesSectored[sh]) {

        var sector = $p.particlesSectored[sh][sv];

        if (sector.length > 0) {
          for (let $p1 in sector) {
            for (let $p2 in sector) {

              // Collision checker
              if ($p1 === $p2) continue;

              if (sector[$p1]) if (sector[$p1].opacity > 0.8) var hasCollision = sector[$p1].detectCollision(sector[$p2]);
              if (hasCollision) {

                sector.splice($p1, 1);

              }
            }
          }
        }
      }
    }

  };

  // Main animation
  $p.animate = function () {

    $p.drawParticles();

    if (!$p.paused) window.requestAnimationFrame($p.animate);

  };

  // Pause animation
  $p.pause = () => {$p.paused = true;};

  $p.play = () => {

    $p.paused = false;

    $p.animate();

    if ($p.firstInit) Tween.to($p.canvas, 3, {

      opacity: 1,
      ease: Power3.easeOut });



    $p.firstInit = false;

  };

  /*////////////////////////////////////////////
     ----	Events
     ///////////////////////////////////////////*/

  // Attach particles to moving object (created originally for mouse movement, thus the var namings)

  $p.attachToObject = (x, y) => {

    // Get movement vector
    let mouseMovementVector = { x: x - $p.mouse.position.x, y: y - $p.mouse.position.y };

    // Get movement vector length
    let mouseMovementVectorLength = $p.getVectorLength(mouseMovementVector);

    // Get current timestamp
    let currentTimestamp = Date.now();

    // Get difference in time
    let deltaTimestamp = currentTimestamp - $p.mouse.timestamp;

    // Calculate mouse speed
    $p.mouse.physics.speed = mouseMovementVectorLength / deltaTimestamp;

    // Calculate mouse acceleration
    $p.mouse.physics.accel = $p.mouse.physics.speed / deltaTimestamp * 100;

    // Get movement vector angle in radians
    $p.mouse.physics.angle = Math.atan2(mouseMovementVector.y, mouseMovementVector.x);

    // Save current timestamp
    $p.mouse.timestamp = currentTimestamp;

    // Save current mouse coordinates
    $p.mouse.position.x = x;
    $p.mouse.position.y = y;

  };

  $p.createParticles();

  return $p;

};

//////////////////////////////////////////////

/*////////////////////////////////////////////
----	React app for demoing the particles
///////////////////////////////////////////*/

// Dragbar
class Dragbar extends React.Component {

  constructor(props) {

    super(props);_defineProperty(this, "_drag",











    e => {

      e = e || window.event;

      let start = 0;let diff = 0;

      if (e.pageX) start = e.pageX;else
      if (e.clientX) start = e.clientX;

      this._animateDraggieText('in');

      document.body.onmousemove = e => {

        e = e || window.event;

        let end = 0;

        if (e.pageX) end = e.pageX;else
        if (e.clientX) end = e.clientX;

        diff = end - start + this.state.startingX;

        if (diff < 0) diff = 0;
        if (diff > this.props.range - 19) diff = this.props.range - 19;

        this.setState({ position: diff });

        this._setValue();

      };

      document.body.onmouseup = () => {

        let position = this.state.position;

        this.setState({ startingX: position });

        document.body.onmousemove = document.body.onmouseup = null;

        if (typeof this.props.onChange === 'function') this.props.onChange(this.state.value);

        this._animateDraggieText('out');
      };

    });_defineProperty(this, "_setValue",

    () => {

      let position = this.state.position;
      let rangeMax = this.props.range - 19;

      let actualRange = this.props.values.max - this.props.values.min;

      let currentValue = position / rangeMax * actualRange + this.props.values.min;

      this.setState({ value: currentValue.toFixed(0) });

    });_defineProperty(this, "_guideClick",

    e => {

      let t = ReactDOM.findDOMNode(this);
      let guide = t.getElementsByClassName('drag-guide')[0];
      let draggie = t.getElementsByClassName('draggie')[0];
      let guideOffsets = guide.getBoundingClientRect();
      let guideLeft = guideOffsets.left;

      e = e || window.event;

      let position = 0;

      if (e.pageX) position = e.pageX;else
      if (e.clientX) position = e.clientX;

      let newPosition = position - guideLeft;
      if (newPosition < 0) newPosition = 0;
      if (newPosition > this.props.range - 19) newPosition = this.props.range - 19;

      let self = this;

      TweenMax.to(

      draggie,
      0.8,
      {

        left: newPosition,
        ease: Power3.easeOut,
        onComplete: () => {

          this._animateDraggieText('out');
          this.props.onChange(this.state.value);

        },
        onStart: () => {this._animateDraggieText('in');},
        onUpdate: function () {

          let left = this.target.style.left;

          left = parseInt(left);

          self.setState({ position: left, startingX: left });

          self._setValue();

        } });





    });_defineProperty(this, "_animateDraggieText",

    where => {

      let values = {};

      switch (where) {

        case 'in':

          values = { opacity: 1, scale: 1, ease: Power3.easeOut };

          break;

        case 'out':

          values = { opacity: 0, scale: 0.5, ease: Power3.easeOut };

          break;}



      TweenMax.to(this.state.draggie.getElementsByTagName('span')[0], 0.8, values);

    });_defineProperty(this, "componentDidMount",

    () => {

      let t = ReactDOM.findDOMNode(this);
      let draggie = t.getElementsByClassName('draggie')[0];

      TweenMax.set(draggie.getElementsByTagName('span'), { opacity: 0, scale: 0.5 });

      this.setState({ draggie: draggie, value: this.props.values.min });

    });this.state = { position: 0, value: 0, startingX: 0, draggie: null };}

  render() {

    return (

      React.createElement("div", { className: "dragbar" },

      React.createElement("span", { className: "dragFrom drag-values" }, this.props.values.min, " ", this.props.unit),
      React.createElement("span", { className: "dragTo drag-values" }, this.props.values.max, " ", this.props.unit),
      React.createElement("div", { className: "drag-guide", onMouseDown: this._guideClick }),
      React.createElement("div", { onMouseDown: this._drag.bind(this), style: { left: this.state.position }, className: "draggie" }, React.createElement("span", null, this.state.value))));





  }}



// Radios
class Radios extends React.Component {

  constructor(props) {

    super(props);_defineProperty(this, "componentDidMount",





    () => {

      let items = [];

      let t = ReactDOM.findDOMNode(this);
      let radios = t.getElementsByClassName('radio');

      for (let i in this.props.items) {

        let item = this.props.items[i];
        items[item.name] = item;

        if (!item.selected) {

          let icon = radios[i].getElementsByTagName('svg');
          TweenMax.set(icon, { x: 50 });

        }

      }

      this.setState({ items: items });

    });_defineProperty(this, "_onClick",

    (name, callback) => {

      let stateItems = this.state.items;
      if (stateItems[name].selected) return;

      let t = ReactDOM.findDOMNode(this);
      let radios = t.getElementsByClassName('radio');
      let index = 0;

      for (let i in stateItems) {

        let item = stateItems[i];
        let tween = false;

        if (item.selected) {

          item.selected = false;
          tween = 50;

        }
        if (item.name === name) {

          item.selected = true;
          tween = 0;

        }

        let icon = radios[index].getElementsByTagName('svg');
        TweenMax.to(

        icon,
        0.8,
        {

          x: tween,
          ease: Power3.easeOut });





        index++;

      }

      callback(stateItems[name]);

    });_defineProperty(this, "_buildRadios",

    (items, clickCallback) => {

      let radios = items.map(item => {

        return React.createElement("div", { className: 'radio radio-' + item.name, onClick: this._onClick.bind(this, item.name, clickCallback) },

        React.createElement("div", { className: "radio-icon" }, React.createElement("svg", { xmlns: "http://www.w3.org/2000/svg", viewBox: "0 0 26 26" }, React.createElement("path", { d: "M.3 14c-.2-.2-.3-.5-.3-.7s.1-.5.3-.7l1.4-1.4c.4-.4 1-.4 1.4 0l.1.1 5.5 5.9c.2.2.5.2.7 0L22.8 3.3h.1c.4-.4 1-.4 1.4 0l1.4 1.4c.4.4.4 1 0 1.4l-16 16.6c-.2.2-.4.3-.7.3-.3 0-.5-.1-.7-.3L.5 14.3.3 14z" }))),
        React.createElement("div", { className: "radio-title" }, item.title));



      });

      return radios;

    });this.state = { items: [] };}

  render() {

    return (

      React.createElement("div", { className: "radios" },

      this._buildRadios(this.props.items, this.props.click)));





  }}



// Canvas
class Canvas extends React.Component {

  constructor(props) {

    super(props);_defineProperty(this, "componentDidMount",





    () => {

      window.onresize = () => {

        this.setState({

          width: window.innerWidth,
          height: window.innerHeight });



      };

    });this.state = { width: window.innerWidth, height: window.innerHeight };}

  render() {

    return (

      React.createElement("canvas", { width: this.state.width, height: this.state.height }));



  }}



// Guide
class Guide extends React.Component {

  render() {

    return (

      React.createElement("div", { className: "guide", style: { left: this.props.position.x - 24, top: this.props.position.y - 24 } },

      React.createElement("div", { className: "dot" }),
      React.createElement("div", { className: "ring ring-one" }),
      React.createElement("div", { className: "ring ring-two" })));





  }}



// Settings
class Settings extends React.Component {

  render() {

    return (

      React.createElement("div", { className: "appSettings" },

      React.createElement("div", { className: "setting setting-theme" },

      React.createElement("span", null, "Theme color:"),
      React.createElement("div", { className: "white", onClick: this.props.colorChange.bind(null, 'white') }),
      React.createElement("div", { className: "grey", onClick: this.props.colorChange.bind(null, 'grey') }),
      React.createElement("div", { className: "dark", onClick: this.props.colorChange.bind(null, 'dark') }),
      React.createElement("div", { className: "orange", onClick: this.props.colorChange.bind(null, 'orange') }),
      React.createElement("div", { className: "blue", onClick: this.props.colorChange.bind(null, 'blue') })),



      React.createElement("div", { className: "setting setting-guide" },

      React.createElement(Radios, { click: this.props.guideActionChange, items: [{ name: 'attached', title: 'Attached to mouse', selected: this.props.guideAttachedToMouse }, { name: 'detached', title: 'Free', selected: !this.props.guideAttachedToMouse }] })),



      React.createElement("div", { className: "setting setting-senseRadius" },

      React.createElement("span", null, "Sensitivity radius:"),
      React.createElement(Dragbar, { range: 280, values: { min: 150, max: 600 }, unit: "px", defaultValue: 150, onChange: this.props.dragbarChange }))));







  }}



// Main
class ParticlesDemo extends React.Component {

  constructor(props) {

    super(props);_defineProperty(this, "componentDidMount",




























    () => {

      this.particles = this._createParticles(this.colors['dark']);

      // Launch particles
      this.particles.play();

      // Add mousemove listener  
      document.onmousemove = e => {

        if (this.state.settings.isGuideAttachedToMouse) {

          this.setState({ guidePosition: { x: e.clientX, y: e.clientY } });
          this.particles.attachToObject(e.clientX, e.clientY);

        }

      };

      // Create new particles instance
      let root = ReactDOM.findDOMNode(this);

      // Animate guides first ring
      TweenMax.to(root.getElementsByClassName('ring-one')[0], 1, {

        scale: 1.1,
        ease: Sine.easeInOut,
        opacity: 1,
        yoyo: true,
        repeat: -1 });



      // Animate guides second ring
      TweenMax.to(root.getElementsByClassName('ring-two')[0], 1, {

        scale: 0.4,
        ease: Sine.easeInOut,
        yoyo: true,
        repeat: -1 });



    });_defineProperty(this, "_createParticles",

    colors => {

      // Create new particles instance
      let root = ReactDOM.findDOMNode(this);
      let cnvs = root.getElementsByTagName('canvas')[0];

      cnvs.getContext('2d').clearRect(0, 0, cnvs.width, cnvs.height);

      return new Particles(cnvs, cnvs.width / 7, 7, 0.2, colors.idle, colors.active, this.state.settings.radius, cnvs);

    });_defineProperty(this, "_onColorChange",

    newColor => {

      let settings = this.state.settings;

      settings.themeColor = newColor;
      this.setState({ settings: settings });

      this.particles.pause();
      delete this.particles;

      this.particles = this._createParticles(this.colors[newColor]);
      this.particles.play();

    });_defineProperty(this, "_onGuideActionChange",

    item => {

      let settings = this.state.settings;

      if (item.name === 'attached') {

        this.state.settings.isGuideAttachedToMouse = true;
        if (this.guideAnimations) {

          this.guideAnimations.x.kill();
          this.guideAnimations.y.kill();

          this.guideAnimations = null;

        }

      } else
      {

        this.state.settings.isGuideAttachedToMouse = false;

        this._animateGuideToSin();

      }

      this.setState({ settings: settings });

    });_defineProperty(this, "_animateGuideToSin",

    () => {

      this.guideAnimations = {};

      let root = ReactDOM.findDOMNode(this);
      let guide = root.getElementsByClassName('guide')[0];

      TweenMax.set(guide, { left: 0, top: '70%' });

      this.guideAnimations.x = TweenMax.to(guide, 10, {

        left: window.innerWidth - 48,
        ease: Power0.easeNone,
        yoyo: true,
        repeat: -1 });



      // Move guide vertically
      this.guideAnimations.y = TweenMax.to(guide, 4.25, {

        top: '30%',
        ease: Sine.easeInOut,
        yoyo: true,
        repeat: -1,
        onUpdate: () => {

          let x = guide.offsetLeft + 24;
          let y = guide.offsetTop + 24;

          this.setState({ guidePosition: { x: x, y: y } });
          this.particles.attachToObject(x, y);

        } });



    });_defineProperty(this, "_dragbarChange",

    val => {

      this.particles.mouse.radius = val;

      let settings = this.state.settings;
      settings.radius = val;

      this.setState({ settings: settings });

    });this.guideAnimations = null;this.state = { guidePosition: { x: 0, y: 0 }, settings: { isGuideAttachedToMouse: true, themeColor: 'orange', radius: 150 } };this.colors = { 'white': { idle: { r: 255, g: 255, b: 255 }, active: { r: 255, g: 255, b: 255 } }, 'grey': { idle: { r: 250, g: 250, b: 250 }, active: { r: 255, g: 23, b: 233 } }, 'orange': { idle: { r: 255, g: 255, b: 255 }, active: { r: 255, g: 255, b: 255 } }, 'dark': { idle: { r: 255, g: 255, b: 255 }, active: { r: 239, g: 255, b: 255 } }, 'blue': { idle: { r: 250, g: 250, b: 250 }, active: { r: 250, g: 250, b: 250 } } };}

  render() {

    return (

      React.createElement("div", { className: "theme-" + this.state.settings.themeColor },

      React.createElement(Canvas, null),
      React.createElement(Guide, { position: this.state.guidePosition }),
      React.createElement(Settings, { colorChange: this._onColorChange, guideActionChange: this._onGuideActionChange, guideAttachedToMouse: this.state.settings.isGuideAttachedToMouse, dragbarChange: this._dragbarChange })));





  }}



/*////////////////////////////////////////////
     ----	Render App
     ///////////////////////////////////////////*/
ReactDOM.render(

React.createElement(ParticlesDemo, null),
document.getElementById('particles'));