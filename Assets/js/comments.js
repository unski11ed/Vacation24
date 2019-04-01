import $ from 'jquery';
import jsViews from 'jsviews';
import dayJs from 'dayjs';

import { ajaxRequest } from './ajaxRequest';
import { initializePaginator } from './paginator';
import { notification } from './notification';

// Extend jQuery prototype
jsViews($);

export function initializeComments($container, objectId, isAdmin, noForm) {
    var isFirst = true;

    var $commentsList = $("<div class='comments_list'></div>");
    var $commentsContainer = $("<div class='comments_container'></div>");

    var commentTemplate = $.templates("#comment");
    var currentPage = 0;

    noForm = noForm || false;

    function getComments(objectId, page) {
        ajaxRequest("/Comments/Get?objectId=" + objectId + "&page=" + page, {}, function (comments) {
            $commentsContainer.empty();

            if (comments.Comments.length === 0) {
                $commentsContainer.append('<p class="info">Brak komentarzy</p>')
            }

            //Append comments
            comments.Comments.forEach(function (comment) {
                $commentsContainer.append(commentTemplate.render({
                    id: comment.Id,
                    userId: comment.UserId,
                    userName: comment.UserDisplayName,
                    deletable: isAdmin || comment.UserId === User.UserId,
                    content: comment.Content,
                    date: dayJs(comment.Data).format('DD.MM.YYYY H:mm')
                }));
            });

            if (isFirst && comments.TotalPages > 1) {
                //Spawn paginator
                initializePaginator($commentsList, function (page) {
                    getComments(objectId, page - 1);
                }, comments.TotalPages);

                isFirst = false;
            }

            currentPage = page;
        }, function (error) {
            Message.Error("Nie udało się załadować komentarzy");
        });
    }

    function buildDom() {
        $commentsList.append($commentsContainer);
        $container.append($commentsList);

        if (!noForm) {
            $container.append($.templates("#commentCreate").render());
        }

        //Add comment EventHandler
        $container.find('form').submit(function (e) {
            e.preventDefault();
            var $form = $(this);
            var text = $form.find('textarea').val();

            if (text.length >= 3) {
                ajaxRequest("/Comments/Add", {
                    PlaceId: objectId,
                    Content: text
                }, function (result) {
                    if (result.Status === ResultType.Success) {
                        //View message
                        notification.success("Comment successfully added.");
                        //Clear input field
                        $form.find('textarea').val('');
                        //fetch current comments list
                        getComments(objectId, currentPage);
                    } else {
                        notification.error(result.Message);
                    }
                    
                }, function () {
                    notification.error("Failed to add a comment.");
                });
            }
            
            return false;
        });
    }

    buildDom();
    getComments(objectId, 0); 
}