//# sourceURL=Coupon/image.js
function demoUpload() {
    var $uploadCrop;

    function readFile(input) {
        if (input.files && input.files[0]) {
            var reader = new FileReader();

            reader.onload = function (e) {
                $('.upload-img-div').removeClass('finished');
                $('.upload-img').removeClass('finished');
                $('.upload-img-div').addClass('ready');
                $('.upload-img').addClass('ready');
                $uploadCrop.croppie('bind', {
                    url: e.target.result
                }).then(function () {
                    console.log('jQuery bind complete');
                });

            }

            reader.readAsDataURL(input.files[0]);
        }
        else {
            swal("Sorry - you're browser doesn't support the FileReader API");
        }
    }

    $uploadCrop = $('.upload-img-canvas').croppie({
        viewport: {
            width: 100,
            height: 100,
            type: 'square',
        },
        showZoomer: true,
        enableResize: true,
        enableExif: true,
        enableMaxBoundary: true
    });

    $('#imgUpload').on('change', function () { readFile(this); });
    $("#imgCropSaveLabel").click(function () {
        $uploadCrop.croppie('result',
            {
                type: "base64",
                size: 'original',
                format: "jpeg",
                quality: 0.9
            }).then(function (base64) {
                //$uploadCrop.croppie('destroy');
                $("#imgCropSave").val(base64);
                $('.upload-img-div').removeClass('ready');
                $('.upload-img-div').addClass('finished');
                $('.upload-img').removeClass('ready');
                $('.upload-img').addClass('finished');
                $(".upload-img .upload-img-preview img").attr("src", base64);
            });
    });
    $("#imgUploadLabel").click(function () {
        $("#imgUpload").click();
    });

    $("#imgDelete").click(function ($event) {
        $("#imgCropSave").val("");
        $(".upload-img .upload-img-preview img").attr("src", "");
        $('.upload-img-div').removeClass('finished');
        $('.upload-img').removeClass('finished');
        $('.upload-img-div').removeClass('ready');
        $('.upload-img').removeClass('ready');
        $uploadCrop.croppie("destroy");
        $event.stopPropagation();
        $event.preventDefault();
    });
}