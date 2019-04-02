import $ from 'jquery';
import urlJoin from 'proper-url-join';
import plupload from 'plupload';

import { ajaxRequest } from './ajaxRequest';
import { notification } from './notification';
import { ResultType } from './consts';

export function imagesEditorInitialise(placeId, $objectContainer) {
    var _this = this;

    function createUploader($button, $container, isMain) {
        isMain = isMain || false;

        var uploader = new plupload.Uploader({
            runtimes: 'html5,flash,silverlight,html4',
            browse_button: $button[0],
            container: $container[0],
            url: '/Photos/Add?isMain='+isMain.toString()+'&placeId=' + placeId,
            multi_selection: !isMain,

            filters: {
                max_file_size: '10mb',
                mime_types: [
                    { title: "Images", extensions: "jpg,png" },
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

    var $containerMainImage = $objectContainer.find('[data-images-main-image]')
    var $containerGallery = $objectContainer.find('[data-images-gallery]');
    var galleryUploader = createUploader(
        $objectContainer.find('[data-images-add-trigger="other"]'),
        $objectContainer.find('[data-images-add-input="other"]'),
        false
    );
    var mainPhotoUploader = createUploader(
        $objectContainer.find('[data-images-add-trigger="main"]'),
        $objectContainer.find('[data-images-add-input="other"]'),
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
            ajaxRequest(getThumbnailsUrlBase + "?objectId=" + placeId, {}, function (list) {
                var template = $.templates("#newPhotoThumbnail");
                var megaImageUrlBase = window.appData.photos.thumbnails.urlMega;

                $containerGallery.empty();
                
                list.forEach(function (image) {
                    // If main image ...
                    if (image.Type === 0) {
                        var imageUrl = urlJoin(megaImageUrlBase, image.Filename);
                        $containerMainImage
                            .empty()
                            .append(
                                '<img src="' + imageUrl + '" />'
                            );
                    } else {
                        var html = template.render({
                            photoUrl: urlJoin(PHOTOS_URL, image.Filename),
                            thumbnailUrl: urlJoin(THUMBNAILS_SMALL, image.Filename),
                            photoId: image.Id
                        });
                        $container.append(html);
                    }
                });
    
                assignThumbnailEvents();
            }, function () {
                notification.error("Failed to fetch images list, try again later.");
            });
        }
    }
}
