$( document ).ready(function() {

    // BS4 Popover

    $('[data-toggle="popover-custom-content"]').each(function (i, obj) {

        $(this).popover({
            html: true,
            placement: 'auto',
            template: '<div class="popover popover-custom" role="tooltip"><div class="arrow"></div><h3 class="popover-header"></h3><div class="popover-body"></div></div>',
            content: function () {
                var id = $(this).attr('popover-id');
                return $('#popover-content-' + id).html();
            }
        });

    });

    // Sidebar Menu

    setTimeout(function () {
        $(".vertical-nav-menu").metisMenu();
    }, 100);


    // Stop Bootstrap 4 Dropdown for closing on click inside

    $('.dropdown-menu').on('click', function (event) {
        var events = $._data(document, 'events') || {};
        events = events.click || [];
        for (var i = 0; i < events.length; i++) {
            if (events[i].selector) {

                if ($(event.target).is(events[i].selector)) {
                    events[i].handler.call(event.target, event);
                }

                $(event.target).parents(events[i].selector).each(function () {
                    events[i].handler.call(this, event);
                });
            }
        }
        event.stopPropagation(); //Always stop propagation
    });


    $('[data-toggle="popover-custom-bg"]').each(function (i, obj) {

        var popClass = $(this).attr('data-bg-class');

        $(this).popover({
            trigger: 'focus',
            placement: 'top',
            template: '<div class="popover popover-bg ' + popClass + '" role="tooltip"><h3 class="popover-header"></h3><div class="popover-body"></div></div>'
        });

    });

    $(function () {
        $('[data-toggle="popover"]').popover();
    });

    $('[data-toggle="popover-custom"]').each(function (i, obj) {

        $(this).popover({
            html: true,
            container: $(this).parent().find('.rm-max-width'),
            content: function () {
                return $(this).next('.rm-max-width').find('.popover-custom-content').html();
            }
        });
    });

    $('body').on('click', function (e) {
        $('[rel="popover-focus"]').each(function () {
            if (!$(this).is(e.target) && $(this).has(e.target).length === 0 && $('.popover').has(e.target).length === 0) {
                $(this).popover('hide');
            }
        });
    });

    // BS4 Tooltips

    $(function () {
        $('[data-toggle="tooltip"]').tooltip();
    });

    $(function () {
        $('[data-toggle="tooltip-light"]').tooltip({
            template: '<div class="tooltip tooltip-light"><div class="tooltip-inner"></div></div>'
        });
    });

    // Drawer

    $('.open-right-drawer').click(function () {
        $(this).addClass('is-active');
        $('.app-drawer-wrapper').addClass('drawer-open');
        $('.app-drawer-overlay').removeClass('d-none');
    });

    $('.drawer-nav-btn').click(function () {
        $('.app-drawer-wrapper').removeClass('drawer-open');
        $('.app-drawer-overlay').addClass('d-none');
        $('.open-right-drawer').removeClass('is-active');
    });

    $('.app-drawer-overlay').click(function () {
        $(this).addClass('d-none');
        $('.app-drawer-wrapper').removeClass('drawer-open');
        $('.open-right-drawer').removeClass('is-active');
    });

    // Mobile menu

    $('.mobile-toggle-sidebar-nav').click(function () {
        $(this).toggleClass('is-active');
        if($('.app-container').hasClass('sidebar-mobile-open')) {
            $('.app-container').removeClass('sidebar-mobile-open');
            $('.app-sidebar-overlay').addClass('d-none');
        } else {
            $('.app-container').addClass('sidebar-mobile-open');
            $('.app-sidebar-overlay').removeClass('d-none');
        }
    });

    $('.app-sidebar-overlay').click(function () {
        $(this).addClass('d-none');
        $('.mobile-toggle-sidebar-nav').click();
    });

    $('.mobile-toggle-header-nav').click(function () {
        $(this).toggleClass('active');
        if($('.app-container').hasClass('header-nav-open')) {
            $('.app-container').removeClass('header-nav-open');
            $('.app-header-overlay').addClass('d-none');
        } else {
            $('.app-container').addClass('header-nav-open');
            $('.app-header-overlay').removeClass('d-none');
        }
    });

    $('.app-header-overlay').click(function () {
        $(this).addClass('d-none');
        $('.mobile-toggle-header-nav').click();
    });

    // Page subnavigation menu

    $('.show-menu-btn').on('click', function () {
        $('.app-inner-layout-page').addClass('app-layout-menu-open');
    });

    $('.close-menu-btn').on('click', function () {
        $('.app-inner-layout-page').removeClass('app-layout-menu-open');
    });

    $('.mobile-app-menu-btn').click(function () {
        $('.hamburger', this).toggleClass('is-active');
        $('.app-inner-layout').toggleClass('open-mobile-menu');
    });

    $('.mobile-toggle-nav').click(function () {
        $(this).toggleClass('is-active');
        $('.app-container').toggleClass('header-mobile-open');
    });

    $(window).on('resize', function(){
        var win = $(this);
        if (win.width() > 991) {
            $('.app-container').removeClass('header-mobile-open');
            $('.mobile-toggle-nav').removeClass('is-active');
        }
    });

    // Responsive

    var resizeClass = function () {
        var win = document.body.clientWidth;
        if (win < 1250) {
            $('.app-container').addClass('closed-sidebar-mobile');
        } else {
            $('.app-container').removeClass('closed-sidebar-mobile');
        }
    };


    $(window).on('resize', function () {
        resizeClass();
    });

    resizeClass();
});

