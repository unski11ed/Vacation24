jQuery.browser = {};

window.Base64 = { _keyStr: "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=", encode: function (e) { var t = ""; var n, r, i, s, o, u, a; var f = 0; e = Base64._utf8_encode(e); while (f < e.length) { n = e.charCodeAt(f++); r = e.charCodeAt(f++); i = e.charCodeAt(f++); s = n >> 2; o = (n & 3) << 4 | r >> 4; u = (r & 15) << 2 | i >> 6; a = i & 63; if (isNaN(r)) { u = a = 64 } else if (isNaN(i)) { a = 64 } t = t + this._keyStr.charAt(s) + this._keyStr.charAt(o) + this._keyStr.charAt(u) + this._keyStr.charAt(a) } return t }, decode: function (e) { var t = ""; var n, r, i; var s, o, u, a; var f = 0; e = e.replace(/[^A-Za-z0-9\+\/\=]/g, ""); while (f < e.length) { s = this._keyStr.indexOf(e.charAt(f++)); o = this._keyStr.indexOf(e.charAt(f++)); u = this._keyStr.indexOf(e.charAt(f++)); a = this._keyStr.indexOf(e.charAt(f++)); n = s << 2 | o >> 4; r = (o & 15) << 4 | u >> 2; i = (u & 3) << 6 | a; t = t + String.fromCharCode(n); if (u != 64) { t = t + String.fromCharCode(r) } if (a != 64) { t = t + String.fromCharCode(i) } } t = Base64._utf8_decode(t); return t }, _utf8_encode: function (e) { e = e.replace(/\r\n/g, "\n"); var t = ""; for (var n = 0; n < e.length; n++) { var r = e.charCodeAt(n); if (r < 128) { t += String.fromCharCode(r) } else if (r > 127 && r < 2048) { t += String.fromCharCode(r >> 6 | 192); t += String.fromCharCode(r & 63 | 128) } else { t += String.fromCharCode(r >> 12 | 224); t += String.fromCharCode(r >> 6 & 63 | 128); t += String.fromCharCode(r & 63 | 128) } } return t }, _utf8_decode: function (e) { var t = ""; var n = 0; var r = c1 = c2 = 0; while (n < e.length) { r = e.charCodeAt(n); if (r < 128) { t += String.fromCharCode(r); n++ } else if (r > 191 && r < 224) { c2 = e.charCodeAt(n + 1); t += String.fromCharCode((r & 31) << 6 | c2 & 63); n += 2 } else { c2 = e.charCodeAt(n + 1); c3 = e.charCodeAt(n + 2); t += String.fromCharCode((r & 15) << 12 | (c2 & 63) << 6 | c3 & 63); n += 3 } } return t } };

(function () {
    jQuery.browser.msie = false;
    jQuery.browser.version = 0;
    if (navigator.userAgent.match(/MSIE ([0-9]+)\./)) {
        jQuery.browser.msie = true;
        jQuery.browser.version = RegExp.$1;
    }
})();

$.fn.serializeObject = function () {
    var o = Object.create(null),
        elementMapper = function (element) {
            element.name = $.camelCase(element.name);
            return element;
        },
        appendToResult = function (i, element) {
            var node = o[element.name];

            if ('undefined' != typeof node && node !== null) {
                o[element.name] = node.push ? node.push(element.value) : [node, element.value];
            } else {
                o[element.name] = element.value;
            }
        };

    $.each($.map(this.serializeArray(), elementMapper), appendToResult);
    return o;
};

function ajax(url, data, success, error) {
    AjaxLoader.Show();

    $.ajax({
        url: url,
        dataType: "json",
        type: "POST",
        contentType: 'application/json',
        data: JSON.stringify(data),
        success: success != undefined ? function (data) { success.call(this, data); AjaxLoader.Hide(); } : function () { },
        error: error != undefined ? function (data) { error.call(this, data); AjaxLoader.Hide(); } : function () { }
    });
}

var User = new (function () {
    this.IsLoggedIn = false;
    this.UserId = 0;
    this.Role = "";

    var str = $.cookie("UserData");
    var _this = this;

    var keyvals = str.split("&");
    keyvals.forEach(function (keyval) {
        var a_keyval = keyval.split("=");
        _this[a_keyval[0]] = a_keyval[1];
    });
});

var AjaxLoader = {
    Show: function () {
        $('.ajax_loader').fadeIn();
    },
    Hide: function () {
        $('.ajax_loader').fadeOut();
    }
}

var QueryString = function () {
    // This function is anonymous, is executed immediately and 
    // the return value is assigned to QueryString!
    var query_string = {};
    var query = window.location.search.substring(1);
    var vars = query.split("&");
    for (var i = 0; i < vars.length; i++) {
        var pair = vars[i].split("=");
        // If first entry with this name
        if (typeof query_string[pair[0]] === "undefined") {
            query_string[pair[0]] = pair[1];
            // If second entry with this name
        } else if (typeof query_string[pair[0]] === "string") {
            var arr = [query_string[pair[0]], pair[1]];
            query_string[pair[0]] = arr;
            // If third or later entry with this name
        } else {
            query_string[pair[0]].push(pair[1]);
        }
    }
    return query_string;
}();

function updateQueryStringParameter(uri, key, value) {
    var re = new RegExp("([?&])" + key + "=.*?(&|$)", "i");
    var separator = uri.indexOf('?') !== -1 ? "&" : "?";
    if (uri.match(re)) {
        return uri.replace(re, '$1' + key + "=" + value + '$2');
    }
    else {
        return uri + separator + key + "=" + value;
    }
}

function jnotify(message, type) {
    type = type === undefined ? 'notice' : type;

    var settingsObject = {
        autoHide : true, // added in v2.0
        clickOverlay : false, // added in v2.0
        MinWidth : 320,
        TimeShown : 4000,
        ShowTimeEffect : 500,
        HideTimeEffect : 500,
        LongTrip :20,
        HorizontalPosition : 'right',
        VerticalPosition : 'bottom',
        ShowOverlay : false,
        ColorOverlay : '#000',
        OpacityOverlay: 0.3,
        DeltaY: -50,
        onClosed : function(){ // added in v2.0
		   
        },
        onCompleted : function(){ // added in v2.0
		   
        }
    };

    switch (type) {
        case 'notice':
            jNotify(message, settingsObject);
            break;

        case 'error':
            jError(message, settingsObject);
            break;

        case 'success':
            jSuccess(message, settingsObject);
            break;
    }
}
var Message = {
    Error: function (message) {
        jnotify(message, 'error');
    },
    Success: function (message) {
        jnotify(message, 'success');
    },
    Notification: function (message) {
        jnotify(message, 'notice');
    }
};

var ResultType = {
    Success: 1,
    Error: 2,
    Info: 3
};


function ShowWindow(templateName, data, options) {
    var $content = $('#wnd_modal'),
        template = $.templates(templateName);

    data = data === undefined ? {} : data;

    $content.html(template.render(data));

    $content.bPopup(options);

    return $content;
}

function ShowHtmlWindow(html, options) {
    var $content = $('#wnd_modal');

    $content.html(html);

    $content.bPopup(options);

    return $content;
}

function CloseWindow() {
    $('#wnd_modal').empty().bPopup().close();
}

function parseJsonDate(dateString) {
    return new Date(parseInt(dateString.substr(6)));
}

function toDoubleDecimal(value) {
    return value.toFixed(2).replace('.', ',');
}

function leadingZero(number) {
    return (number < 10 ? "0" : "") + number;
}

function Paginator($container, changeCallback, pagesCount) {
    if (pagesCount <= 1)
        return;

    //Build DOM
    var $parent = $("<div class='paginator'></div>");

    $container.append($parent);

    var $prefix = $("<div class='pagination_prefix' style='display: none'></div>");
    var $postfix = $("<div class='pagination_postfix'  style='display: none'></div>");
    var $pages_container = $("<div class='pages_numbers'></div>");

    $parent.append($prefix)
           .append($pages_container)
           .append($postfix);

    $prefix.append("<a href='#' class='to_begin'><<</a><a href='#' class='prev'><</a>");
    $postfix.append("<a href='#' class='next'>></a><a href='#' class='to_end'>>></a>");


    for (var i = 1; i <= pagesCount; i++) {
        $pages_container.append("<a href='#' class='to_page'>" + i + "</a>");
    }

    //Set first page active
    $pages_container.find('a').first().addClass('active');
    HandleEdgeVisibility();

    //Event Handlers

    //Prefix
    $prefix.find('.to_begin').click(function () {
        $pages_container.children('a').removeClass('active')
                        .first().addClass('active');

        HandleEdgeVisibility();

        changeCallback.call(window, 1);

        return false;
    });

    $prefix.find('.prev').click(function () {
        var $currentElement = $pages_container.find('a.active');
        var $prevElement = $currentElement.prev();

        if ($prevElement.length > 0) {
            $currentElement.removeClass('active');
            $prevElement.addClass('active');

            HandleEdgeVisibility();

            changeCallback.call(window, $prevElement.text());
        }
        return false;
    });

    //Postfix
    $postfix.find('.to_end').click(function () {
        var $lastPage = $pages_container.children('a').removeClass('active')
                                        .last().addClass('active');

        HandleEdgeVisibility();

        changeCallback.call(window, $lastPage.text());

        return false;
    });

    $postfix.find('.next').click(function () {
        var $currentElement = $pages_container.find('a.active');
        var $nextElement = $currentElement.next();

        if ($nextElement.length > 0) {
            $currentElement.removeClass('active');
            $nextElement.addClass('active');

            HandleEdgeVisibility();

            changeCallback.call(window, $nextElement.text());
        }
        return false;
    });

    //Page numbers
    $pages_container.children('a').click(function () {
        $pages_container.children('a').removeClass('active');
        $(this).addClass('active');

        HandleEdgeVisibility();

        changeCallback.call(window, $(this).text());

        return false;
    });

    function HandleEdgeVisibility() {
        var $active = $pages_container.find('a.active');

        if ($active.index() === $pages_container.children().length - 1)
            $postfix.hide();
        else
            $postfix.show();

        if ($active.index() === 0)
            $prefix.hide();
        else
            $prefix.show();
    }

};

PlaceOptions = {
    FindByModelName: function (modelName) {
        var output = null;
        PLACE_OPTIONS_DEFINITION.forEach(function (def) {
            if (def.modelName == modelName)
                output = def;
        });
        return output;
    },

    GetAll: function () {
        return PLACE_OPTIONS_DEFINITION;
    }
};

function updateQueryStringParameter(uri, key, value) {
    var re = new RegExp("([?&])" + key + "=.*?(&|$)", "i");
    var separator = uri.indexOf('?') !== -1 ? "&" : "?";
    if (uri.match(re)) {
        return uri.replace(re, '$1' + key + "=" + value + '$2');
    }
    else {
        return uri + separator + key + "=" + value;
    }
}