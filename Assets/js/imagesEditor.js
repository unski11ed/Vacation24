/*
DOM elements:

data-images-add-input="other"
data-images-add-trigger="other"
data-images-add-input="main"
data-images-add-trigger="main"
data-images-gallery
*/
import $ from 'jquery';

import { ajaxRequest } from './ajaxRequest';
import { notification } from './notification';
import { ResultType } from './consts';

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

    var $containerMainImage = $container.find('[data-images-main-image]')
    var $containerGallery = $container.find('[data-images-gallery]');
    var galleryUploader = createUploader(
        $container.find('[data-images-add-trigger="other"]'),
        $container.find('[data-images-add-input="other"]'),
        false
    );
    var mainPhotoUploader = createUploader(
        $container.find('[data-images-add-trigger="main"]'),
        $container.find('[data-images-add-input="other"]'),
        true
    );

    galleryUploader.init();
    mainPhotoUploader.init();

    // Assign Delete Event Handler
    $(document).on('click', '[data-images-gallery] .delete_photo', function() {
        var $thumbnail = $(this).closest('.photo_thumb');

        if (
            $thumbnail.length > 0 &&
            confirm("Are you sure you want to delete this photo?")
        ) {
            var deleteUrlBase = window.appData.photos.urls.remove;
            ajaxRequest(deleteUrlBase + '?photoId=' + $thumbnail.data('photoId'), {}, function (result) {
                if (result.Status === ResultType.Success) {
                    $thumbnail.remove();
                } else {
                    notification.error("Failed to remove photo - Database Errror.");
                }
            }, function () {
                notification.error("Failed to remove photo - connection error.");
            });
        }

        return false;
    });

    return {
        loadThumbnails: function() {
            var getThumbnailsUrlBase = window.appData.photos.urls.getThumbnails;
            ajax(getThumbnailsUrlBase + "?objectId=" + _placeId, {}, function (list) {
                var template = $.templates("#newPhotoThumbnail");
                var megaImageUrlBase = window.appData.photos.thumbnails.urlMega;

                $containerGallery.empty();
                
                list.forEach(function (image) {
                    if (image.Type === 0) {
                        $containerMainImage
                            .empty()
                            .append(
                                '<img src="' + megaImageUrlBase + '/' + image.Filename + '" />'
                            );
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
    }
}
