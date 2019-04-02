/*
DOM elements:

data-images-add-input="other"
data-images-add-trigger="other"
data-images-add-input="main"
data-images-add-trigger="main"
data-images-gallery
*/

export function imagesEditorInitialise(placeId, $objectContainer) {
    

    var _placeId = placeId;
    var _this = this;

    function createUploader($button, $container, isMain) {
        isMain = isMain || false;

        var uploader = new plupload.Uploader({
            runtimes: 'html5,flash,silverlight,html4',
            browse_button: $button[0],
            container: $container[0],
            url: '/Photos/Add?isMain='+isMain.toString()+'&placeId=' + placeId,
            flash_swf_url: '/Scripts/plupload/Moxie.swf',
            silverlight_xap_url: '/Scripts/plupload/Moxie.xap',
            multi_selection: !isMain,

            filters: {
                max_file_size: '10mb',
                mime_types: [
                    { title: "Obrazy", extensions: "jpg,png" },
                ]
            },

            init: {
                FilesAdded: function (up, files) {
                    up.start();
                    AjaxLoader.Show();
                },

                FileUploaded: function (up, file, info) {
                    AjaxLoader.Hide();
                    _this.LoadThumbnails();
                },

                Error: function (up, err) {
                    Message.Error("Wystąpił błąd podczas przesyłania pliku. Spróbuj ponownie.");
                    AjaxLoader.Hide();
                }
            }
        });
        return uploader;
    }


    var uploader = createUploader($('#add_gallery_image'), $('#tabgaleria #galeria_input'), false);
    var mainPhotoUploader = createUploader($('.addimg'), $('.addimg').parent(), true);

    uploader.init();
    mainPhotoUploader.init();

    this.LoadThumbnails = function () {
        ajax("/Photos/GetThumbnails?objectId=" + _placeId, {}, function (list) {
            var $container = $('#tabgaleria .images_grasp'),
                template = $.templates("#newPhotoThumbnail");

            $container.empty();

            list.forEach(function (image) {
                if (image.Type === 0) {
                    $('.addimg').empty().append('<img src="' + THUMBNAILS_MEGA + '/' + image.Filename + '" />');
                } else {
                    var html = template.render({
                        photoUrl: PHOTOS_URL + '/' + image.Filename,
                        thumbnailUrl: THUMBNAILS_SMALL + image.Filename,
                        photoId: image.Id
                    });
                    $container.append(html);
                }
            });

            assignThumbnailEvents();
        }, function () {
            Message.Error("Nie udało się pobrać listy zdjęć. Spróbuj jeszcze raz.");
        });
    }

    var assignThumbnailEvents = function () {
        var $thumbnails = $('#tabgaleria .images_grasp .photo_thumb');

        $thumbnails.find('.delete_photo').click(function () {
            var $thumbnail = $(this).closest('.photo_thumb');

            if (confirm("Czy na pewno usunąć to zdjęcie?")) {
                ajax("/Photos/Delete?photoId=" + $thumbnail.data('photoId'), {}, function (result) {
                    if (result.Status === ResultType.Success) {
                        $thumbnail.remove();
                    } else {
                        Message.Error("Nie udało się usunąć zdjęcia.");
                    }
                }, function () {
                    Message.Error("Nie udało się usunąć zdjęcia. Problemy z połączeniem.");
                });
            }
            return false;
        });
    }
}
