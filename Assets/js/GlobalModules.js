function Comments($container, objectId, isAdmin, noForm) {
    var isFirst = true;

    var $commentsList = $("<div class='comments_list'></div>");
    var $commentsContainer = $("<div class='comments_container'></div>");

    var commentTemplate = $.templates("#comment");
    var currentPage = 0;

    noForm = noForm || false;

    function getComments(objectId, page) {
        ajax("/Comments/Get?objectId=" + objectId + "&page=" + page, {}, function (comments) {
            $commentsContainer.empty();

            if (comments.Comments.length === 0) {
                $commentsContainer.append('<p class="info">Brak komentarzy</p>')
            }

            //Append comments
            comments.Comments.forEach(function (comment) {
                var date = parseJsonDate(comment.Date);
                $commentsContainer.append(commentTemplate.render({
                    id: comment.Id,
                    userId: comment.UserId,
                    userName: comment.UserDisplayName,
                    deletable: isAdmin || comment.UserId === User.UserId,
                    content: comment.Content,
                    date: leadingZero(date.getUTCDate()) + "." + leadingZero(date.getUTCMonth() + 1)
                          + "." + leadingZero(date.getFullYear()) + " " + leadingZero(date.getHours())
                          + ":" + leadingZero(date.getMinutes())
                }));
            });
            assignEventHandlers();

            if (isFirst && comments.TotalPages > 1) {
                //Spawn paginator
                Paginator($commentsList, function (page) {
                    getComments(objectId, page - 1);
                }, comments.TotalPages);

                isFirst = false;
            }

            currentPage = page;
        }, function (error) {
            Message.Error("Nie udało się załadować komentarzy");
        });
    }

    function assignEventHandlers() {

    }

    function buildDom() {
        $commentsList.append($commentsContainer);
        $container.append($commentsList);

        //if (User.IsLoggedIn !== "False" && !noForm)
        if (!noForm)
            $container.append($.templates("#commentCreate").render());
        //else if (User.IsLoggedIn === "False" && !noForm)
        //    $container.append($.templates("#commentForbidden").render());

        //Add comment EventHandler
        $container.find('form').submit(function (e) {
            e.preventDefault();
            var $form = $(this);
            var text = $form.find('textarea').val();

            if (text.length >= 3) {
                ajax("/Comments/Add", {
                    PlaceId: objectId,
                    Content: text
                }, function (result) {
                    if (result.Status === ResultType.Success) {
                        //View message
                        Message.Success("Pomyślnie dodano komentarz.");
                        //Clear input field
                        $form.find('textarea').val('');
                        //fetch current comments list
                        getComments(objectId, currentPage);
                    } else {
                        Message.Error(result.Message);
                    }
                    
                }, function () {
                    Message.Error("Nie udało się dodać komentarza");
                });
            }
            
            return false;
        });
    }

    buildDom();
    getComments(objectId, 0); 
}

function DisplayObjectOptions($container, options) {
    for (var prop in options) {
        if (options.hasOwnProperty(prop) && options[prop]) {
            var option = PlaceOptions.FindByModelName(prop);
            if (option) {
                $container.append(
                    '<span class="option_icon ' + option.iconClass + ' " rel="tipsy" title="' + option.title + '"></span>'
                );
            }
        }
    }
}

function MapDisplay($container, lattitude, longitude) {
    var directionsService = new google.maps.DirectionsService();
    var directionsDisplay = new google.maps.DirectionsRenderer();

    var centerPoint = new google.maps.LatLng(lattitude, longitude);

    var marker = null;

    //Settings
    var noStreetNames = [{
        featureType: "road",
        elementType: "labels",
        stylers: [{
            visibility: "off"
        }]
    }];

    hideLabels = new google.maps.StyledMapType(noStreetNames, {
        name: "hideLabels"
    });

    var myOptions = {
        zoom: 6,
        mapTypeId: google.maps.MapTypeId.ROADMAP,
        center: centerPoint
    }

    //Create map
    var map = new google.maps.Map($(".map_go")[0], myOptions);
    directionsDisplay.setMap(map);
    map.mapTypes.set('hide_street_names', hideLabels);

    //Add marker
    marker = new google.maps.Marker({
        position: centerPoint,
        title: $(".objecttitle").val(),
        map: map
    });

    map.setCenter(marker.getPosition());

    this.Refresh = function () {
        google.maps.event.trigger(map, 'resize');

        map.setCenter(marker.getPosition());
    }
}


function ContactForm($form, objectId) {
    var _this = this;

    $form.submit(function (e) {
        e.preventDefault();

        var content = $(this).find("textarea[name=Content]").val(),
            subject = $(this).find("input[name=Subject]").val(),
            email = $(this).find("input[name=Email]").val();

        if (content.length >= 3 && subject.length >= 3) {
            _this.Send(subject, content, email, objectId);

            $(this).find("input[type=submit]").attr('disabled', true);
        }

        return false;
    });

    this.Send = function (subject, content, email, id) {
        ajax("/Messages/Send", {
            Subject: subject,
            Content: content,
            ObjectId: id,
            Email: email
        }, function (data) {
            if (data.Status == ResultType.Success) {
                Message.Success(data.Message);
                $form.find('textarea,input[type=text],input[type=email]').val('');
            }
        }, function () {
            Message.Error("Nie udało się wysłać wiadomości, spróbuj jeszcze raz.");
        });
    }
}

function Stash() {
    var STASH_LIMIT = 10;
    var _this = this;

    var list = [];

    function save() {
        $.jStorage.set("stash", JSON.stringify(list));
    }

    function load() {
        list = JSON.parse($.jStorage.get("stash"));
        if (!list)
            list = [];
    }

    this.OnStashChanged = function () { }

    this.Add = function (id) {
        if (list.length >= STASH_LIMIT) {
            Message.Notification("Schowek pełny, zwolnij miejsce i spróbuj jeszcze raz.");
        }

        //Unikaj duplikatów
        if (list.filter(function (element) { return element.Id == id }).length)
            return;

        ajax("/Stash/Get", {
            Id: id
        }, function (data) {
            if (data.status === ResultType.Success) {
                list.push(data.item);
                save();
                _this.OnStashChanged.call(window);
            }
        });
    }

    this.List = function () {
        return list;
    }

    this.Remove = function (id) {
        for (var i = 0; i < list.length; i++) {
            var element = list[i];
            if (element.Id == id) {
                list.splice(i, 1);
                save();
                _this.OnStashChanged.call(window);
                break;
            }
        }
    }

    this.GetCounter = function () {
        return list.length;
    }

    load();
}

function VideoDisplay(placeId, $videoContainer) {
    var _this = this;

    this.load = function () {
        ajax("/Videos/View", { Id: placeId }, function (result) {
            if (result && result.EmbedUrl) {
                $videoContainer.parent()
                               .find('input')
                               .val(result.OriginalUrl);
                _this.embed(result.EmbedUrl, $videoContainer);
            } else {
                $container.empty();
            }
        });
    };

    this.embed = function (id, $container) {
        $container = $container || $videoContainer;

        var markup = '<iframe width="620" height="349" src="https://www.youtube.com/embed/' + id + '" frameborder="0" allowfullscreen></iframe>';
        $container.empty().append(markup);
    }
}