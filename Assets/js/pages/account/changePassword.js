import $ from 'jquery';

$('[data-href]').click(function () {
    window.location.href = $(this).data('href');
});