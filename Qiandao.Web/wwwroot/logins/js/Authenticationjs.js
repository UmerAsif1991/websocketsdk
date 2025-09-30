var OnBegin = function () {

}
var OnLoginSuccess = function (data) {
    console.log(data);
    $.validator.unobtrusive.parse('form');
    if (data.success == false) {
        swal({
            title: "Ooops... !",
            type: "warning",
            showCancelButton: false,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "Ok",
            text: data.errorMessage[0]
        });
    }
    if (data.success == true) {
        window.location.href = '/Home/Index'
    }
}