import $ from 'jquery';

export function initializePaginator($container, changeCallback, pagesCount) {
    if (pagesCount <= 1)
        return;

    //Build DOM
    var $parent = $("<div class='paginator'></div>");

    $container.append($parent);

    var $prefix = $("<div class='pagination_prefix' style='display: none'></div>");
    var $postfix = $("<div class='pagination_postfix'  style='display: none'></div>");
    var $pagesContainer = $("<div class='pages_numbers'></div>");

    $parent
        .append($prefix)
        .append($pagesContainer)
        .append($postfix);

    $prefix.append("<a href='#' class='to_begin'><<</a><a href='#' class='prev'><</a>");
    $postfix.append("<a href='#' class='next'>></a><a href='#' class='to_end'>>></a>");

    for (var i = 1; i <= pagesCount; i++) {
        $pagesContainer.append("<a href='#' class='to_page'>" + i + "</a>");
    }

    //Set first page active
    $pagesContainer.find('a').first().addClass('active');
    HandleEdgeVisibility();

    //Event Handlers

    //Prefix
    $prefix.find('.to_begin').click(function () {
        $pagesContainer.children('a').removeClass('active')
                        .first().addClass('active');

        handleEdgeVisibility();

        changeCallback.call(window, 1);

        return false;
    });

    $prefix.find('.prev').click(function () {
        var $currentElement = $pagesContainer.find('a.active');
        var $prevElement = $currentElement.prev();

        if ($prevElement.length > 0) {
            $currentElement.removeClass('active');
            $prevElement.addClass('active');

            handleEdgeVisibility();

            changeCallback.call(window, $prevElement.text());
        }
        return false;
    });

    //Postfix
    $postfix.find('.to_end').click(function () {
        var $lastPage = $pagesContainer.children('a').removeClass('active')
                                        .last().addClass('active');

        handleEdgeVisibility();

        changeCallback.call(window, $lastPage.text());

        return false;
    });

    $postfix.find('.next').click(function () {
        var $currentElement = $pagesContainer.find('a.active');
        var $nextElement = $currentElement.next();

        if ($nextElement.length > 0) {
            $currentElement.removeClass('active');
            $nextElement.addClass('active');

            handleEdgeVisibility();

            changeCallback.call(window, $nextElement.text());
        }
        return false;
    });

    //Page numbers
    $pagesContainer.children('a').click(function () {
        $pagesContainer.children('a').removeClass('active');
        $(this).addClass('active');

        handleEdgeVisibility();

        changeCallback.call(window, $(this).text());

        return false;
    });

    function handleEdgeVisibility() {
        var $active = $pagesContainer.find('a.active');

        if ($active.index() === $pagesContainer.children().length - 1) {
            $postfix.hide();
        } else {
            $postfix.show();
        }

        if ($active.index() === 0) {
            $prefix.hide();
        } else {
            $prefix.show();
        }
    }
};
