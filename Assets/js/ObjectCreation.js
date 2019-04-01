function FormFiller($targetForm){
    var $_form = $targetForm;

    this.Set = function(data){
        for (var k in data) {
            if (data.hasOwnProperty(k)) {
                var $field = $_form.find("[name='" + k + "']");

                if ($field.is(":checkbox") && data[k]) {
                    $field.attr("checked", true);
                    continue;
                }

                if (data[k] !== null)
                    $_form.find("[name='" + k + "']").val(data[k]).trigger("change");
            }
        }
    },

    this.Get = function () {
        var output = {};

        $($targetForm).find('input,select,textarea').each(function () {
            var $element = $(this);

            var value;
            if ($element.is(':checkbox')) {
                value = $element.is(':checked');
            }
            else {
                value = $element.val();
            }

            output[$element.attr('name')] = value;
        });


        return output;
    }
}

function PlaceEditor(placeId) {
    var _placeId = placeId;
    var _formFiller = new FormFiller($('#place_edit'));

    this.Place = null;

    this.Load = function (callbackComplete) {
        ajax("/Object/Get", _placeId != undefined ? { id: _placeId } : null, function (place) {
            this.Place = place;
            
            //Fill form
            if(_placeId !== undefined)
                _formFiller.Set(this.Place);

            callbackComplete.call(window, place);
        });
    },

    this.Save = function (callbackPreSave, callbackPostSave) {
        callbackPostSave = callbackPostSave || function () { };

        var data = _formFiller.Get();
        if (_placeId) {
            data.Id = _placeId;
        }

        callbackPreSave.call(window, data);

        ajax("/Object/Save", data, function (result) {
            if (result.Status === ResultType.Success) {
                Message.Success("Pomyślnie uaktualniono obiekt");
                callbackPostSave.call(window, data);
            } else {
                Message.Error("Błąd: " + result.message);
            }
        });
    }

    this.IsInitialised = function () { return this.Place != null; }
}

function ImagesEditor(placeId) {
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

function MapEditor() {
    var directionsService = new google.maps.DirectionsService();
    var directionsDisplay = new google.maps.DirectionsRenderer();

    var centerPoint = new google.maps.LatLng(52.0692914, 19.4802122);

    var marker = null;

    //======================Constructor=========================
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

    var map = new google.maps.Map($(".map_go")[0], myOptions);
    directionsDisplay.setMap(map);
    map.mapTypes.set('hide_street_names', hideLabels);


    //===============Private functions===========================

    function setMarker(point) {
        if (marker === null) {
            marker = new google.maps.Marker({
                position: point,
                title: $(".objecttitle").val(),
                draggable: true,
                map: map
            });

            map.setCenter(marker.getPosition());

            google.maps.event.addListener(marker, 'dragend', savePosition);
        } else {
            marker.setPosition(point);
        }
        savePosition();
    }

    function savePosition() {
        var lat = marker.getPosition().lat(),
            lng = marker.getPosition().lng();

        $("#objectlattitude").val(lat);
        $("#objectlongitude").val(lng);
    }

    this.Update = function () {
        var lattitude = $("#objectlattitude").val(),
            longitude = $("#objectlongitude").val();

        //Marker in center if no Lat/Lng set in model
        var markerPoint = (lattitude == "0" || longitude == "0") || (lattitude == "" || longitude == "")
                            ? centerPoint : new google.maps.LatLng(parseFloat(lattitude), parseFloat(longitude));

        setMarker(markerPoint);
    }

    this.Refresh = function () {
        google.maps.event.trigger(map, 'resize');

        map.setCenter(marker.getPosition());
    }
}


function PlaceOptionsEditor(placeId) {
    var _this = this;
    var _optionsModel = { };

    function spawnManagementWindow(callbackOk) {
        var data = {
            availableOptions: PlaceOptions.GetAll(),
            placeOptions: getObjectOptions()
        }
        $wnd = ShowWindow("#placeOptionsWindow", data, {});

        refreshTipsy();

        $wnd.find(".available_options .option_icon").click(function () {
            var $targetContainer = $wnd.find(".place_options");

            //Check if not in container, to prevent duplicates
            if ($targetContainer.find("[data-model-name='" + $(this).data('modelName') + "']").length == 0) {
                var $clone = $(this).clone();

                $clone.appendTo($targetContainer)
                      .click(function () {
                          $(this).remove();
                      });
            }
        });

        $wnd.find(".place_options .option_icon").click(function () {
            $(this).remove();
        });

        $wnd.find(".cmd_ok").click(function () {
            var options = [];
            $wnd.find(".place_options .option_icon").each(function () {
                var option = PlaceOptions.FindByModelName($(this).data("modelName"));
                options.push(option);
            });
            callbackOk.call(window, options);

            CloseWindow();

            return false;
        });

        $wnd.find(".cmd_cancel").click(function () {
            CloseWindow();

            return false;
        });
    }

    function getObjectOptions() {
        var options = [];

        $('.objecticonslist .option_icon').each(function () {
            var $icon = $(this);

            var optionModelName = $icon.data("modelName");
            var optionDefinition = PlaceOptions.FindByModelName(optionModelName);

            options.push(optionDefinition);
        });

        return options;
    }

    function appendToIconList(options) {
        $('.objecticonslist').empty();
        options.forEach(function (option) {
            $('.objecticonslist').append("<span class='option_icon " + option.iconClass + "' rel='tipsy' original-title='" + option.title + "' data-model-name='" + option.modelName + "'></span>");
        });
        refreshTipsy();
    }

    this.ShowManagementWindow = function () {
        spawnManagementWindow(appendToIconList);
    };

    this.Assign = function (data) {
        //Store loaded model
        _optionsModel = data === null ? {} : data;

        var optionsToAppend = [];

        for (var prop in data) {
            if (data.hasOwnProperty(prop) && data[prop]) {
                var o = PlaceOptions.FindByModelName(prop);
                if (o !== null) {
                    optionsToAppend.push(o);
                }
            }
        }

        appendToIconList(optionsToAppend);
    };

    this.FillModelData = function (input) {
        PlaceOptions.GetAll().forEach(function (option) {
            _optionsModel[option.modelName] = false;
            $('.objecticonslist .option_icon').each(function () {
                var modelName = $(this).data('modelName');
                if (option.modelName === modelName) {
                    _optionsModel[modelName] = true;
                }
            });
        });

        input.Options = _optionsModel;

        return input;
    };

    $(".addicons").click(function () {
        _this.ShowManagementWindow();
        return false;
    });
}

function PricesEditor() {
    var _priceListModel = [];
    var containerTemplate = $.templates("#priceElements");
    var $parent = $('.prices_wrap');
    var elementTemplate = $.templates('#priceElement');

    function assignCommandEventHandlers() {
        $('.price_remove').unbind().click(function () {
            $(this).closest('.price_row').remove();

            return false;
        });

        $('.price_edit').unbind().click(function () {
            var $parent = $(this).closest('.price_row');

            $(this).hide();
            $parent.find('.price-edit-command').show();

            $parent.toggleClass('edit-mode');
            //Remove readonly on inputs and backup existing data in case of 'Cancel' command
            $parent.find('input')
                   .removeAttr('readonly')
                   .each(function () {
                       $(this).data('lastValue', $(this).val());
                   });
            return false;
        });

        $('.price_update').unbind().click(function () {
            var $parent = $(this).closest('.price_row');

            $parent.find('input').attr('readonly', 'readonly');
            $('.price_edit').show();
            $('.price-edit-command').hide();

            $parent.toggleClass('edit-mode');

            return false;
        });

        $('.price_cancel').unbind()
                          .click(function () {
            var $parent = $(this).closest('.price_row');

            $parent.find('input')
                    .attr('readonly', 'readonly')
                    .each(function () {
                        $(this).val($(this).data('lastValue'));
                    })
                    .data('lastValue', '');

            $('.price_edit').show();
            $('.price-edit-command').hide();

            $parent.toggleClass('edit-mode');
            return false;
        });
    }

    function updateTemplate(priceList) {
        $parent.find('.price_list').remove();

        $parent.prepend(containerTemplate.render({ prices: priceList, isEditable: true }, {
            toDoubleDecimal: toDoubleDecimal
        }));

        assignCommandEventHandlers();

        $('form#pricesEditor').unbind().submit(function (e) {
            e.preventDefault();

            $(this).find('.edit-mode').removeClass('edit-mode');

            return false;
        });
    }

    this.Assign = function (priceList) {
        _priceListModel = priceList;

        updateTemplate(priceList);
    }

    this.FillModelData = function (data) {
        var list = [];

        var $list = $parent.find('.price_list');
        $list.find('.price_row').each(function () {
            var name = $(this).find('.price_name').val();
            var value = $(this).find('.price_value').val().replace(',', '.');
            var duration = $(this).find('.price_duration').val();

            list.push({Name: name, Value: parseFloat(value), Duration: duration});
        });

        data.Prices = list;
    }

    //Module events
    var $inputContainer = $('.add_price');

    $inputContainer.find(".price_add").click(function () {
        var name = $inputContainer.find('.in_price_name').val(),
            value = $inputContainer.find('.in_price_value').val(),
            duration = $inputContainer.find('.in_price_duration').val();

        if (name === '' || value === '' || duration === '')
            return false;

        var $list = $('.price_list').find('tbody');

        //var html = elementTemplate.render({
        //    Name: name,
        //    Value: parseFloat(value.replace(',', '.')).toFixed(2).replace('.', ','),
        //    Duration: duration,
        //    isEditable: true
        //}, {
        //    toDoubleDecimal: toDoubleDecimal
        //});
        $inputContainer.find('input').val('');

        var html = "<tr class='price_row'>"
                   +"   <td>"
                   +"       <input class=\"price_name\" type=\"text\" value=\"{{name}}\" placeholder=\"Typ pokoju\" readonly=\"readonly\" required/>"
                   +"   </td>"

                   +"   <td>"
                   +"       <input class=\"price_duration\" type=\"text\" value=\"{{duration}}\" placeholder=\"Okres\" readonly=\"readonly\" required />"
                   +"   </td>"

                   +"   <td>"
                   +"       <input class=\"price_value\" type=\"text\" value=\"{{value}}\" placeholder=\"Cena\" readonly=\"readonly\" required />"
                   +"       <span> zł</span>"
                   +"   </td>"

                   +"   <td class=\"price-commands\">"
                   +"       <a href=\"#\" style=\"display: none\" class=\"price-edit-command price_update\">Zapisz</a>"
                   +"       <a href=\"#\" style=\"display: none\" class=\"price-edit-command price_cancel\">Anuluj</a>"
                   +"       <a href=\"#\" class=\"price_edit\">Edytuj</a>"
                   +"       <a href=\"#\" class=\"price_remove\">Usuń</a>"
                   +"   </td>"
                   +"</tr>";
        
        var $newElement = $(html.replace("{{name}}", name)
                              .replace("{{value}}", parseFloat(value.replace(',', '.')).toFixed(2).replace('.', ','))
                              .replace("{{duration}}", duration));

        //var $newElement = $(html);

        $list.append($newElement);

        assignCommandEventHandlers();

        return false;
    });
}

function AdditionalOptionsManager($elements) {
    var $storage = $("input[name=AdditionalOptions]");

    this.Load = function () {
        var selectedValues = $storage.val().split(';');
        selectedValues.forEach(function (value) {
            $elements.filter("[data-value='" + value + "']").attr("checked", 1);
        });
    }
    
    $elements.change(function () {
        var val = "";
        $elements.each(function () {
            if($(this).is(":checked"))
                val += $(this).data('value') + ";"
        });

        $storage.val(val);
    });
}

function VideoManager(objectId, $container) {
    var videoDisplay = new VideoDisplay(objectId, $container.find('.video_grasp'));

    this.init = function () {
        videoDisplay.load();
    }

    $container.find('button.cmd-submit-video').on('click', function () {
        var $input = $container.find('input');
        var url = $input.val();
        if (url) {
            ajax("/Videos/Save", {postId: objectId, url: url}, function (result) {
                if (result.Status === ResultType.Success) {
                    videoDisplay.embed(result.Message);
                } else {
                    Message.Error("Błąd dodawania filmu: " + result.message);
                }
            });
        }
    });

    $container.find('button.cmd-clear-video').on('click', function () {
        ajax("/Videos/Clear", { Id: objectId }, function (result) {
            if (result.Status === ResultType.Success) {
                $container.find('input#youtube-link-input').val('');
                $container.find('.video_grasp').empty();
            }
        });
    });
}

function ObjectCreationInit(objectId) {
    var placeEditor = new PlaceEditor(objectId);
    var photosEditor = new ImagesEditor(objectId);
    var commentsModule = new Comments($("#tabkomentarze .comments_wrap"), objectId, true, true);
    var mapEditor = new MapEditor();
    var placeOptionsEditor = new PlaceOptionsEditor(objectId);
    var pricesEditor = new PricesEditor();
    var additionalOptionsManager = new AdditionalOptionsManager($(".additionalOption"));
    var videoManager = new VideoManager(objectId, $('.video-wrap'));

    //When object data loaded =>
    placeEditor.Load(function (data) {
        //Update map marker
        mapEditor.Update();
        placeOptionsEditor.Assign(data.Options);
        pricesEditor.Assign(data.Prices);
        videoManager.init();

        if (tinymce,editors && tinymce.editors.length > 0){
            tinymce.editors[0].setContent(data.Description ? data.Description : '');
        }else{
            $('textarea#description_extended').val(data.Description);
        }
        //editors.Load();

        additionalOptionsManager.Load();

        $("select").selectbox('detach').selectbox();
    });

    photosEditor.LoadThumbnails();

    $('#place_edit').submit(function (e) {
        e.preventDefault();

        if (this.checkValidity()) {
            placeEditor.Save(function (data) {
                placeOptionsEditor.FillModelData(data);
                pricesEditor.FillModelData(data);

                data.Description = (tinymce.editors.length > 0) ? 
                                        tinymce.editors[0].getContent() :
                                        $('textarea#description_extended').val();
            }, function () {
                if (window.REDIRECT_URL) {
                    window.location.href = window.REDIRECT_URL;
                }
            });
        }

        return false;
    });

    $("#tab-container").bind('easytabs:after', function (e, $tab, $container) {
        if ($container.find('.map_wrap').length > 0)
            mapEditor.Refresh();
    });
};