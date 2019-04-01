import { ajaxRequest } from './ajaxRequest';

export function videoDisplayInitialize(placeId, $videoContainer) {
    var _this = this;

    return {
        load: function() {
            ajaxRequest('/Videos/View', { Id: placeId }, function (result) {
                if (result && result.EmbedUrl) {
                    $videoContainer.parent()
                                   .find('input')
                                   .val(result.OriginalUrl);
                    _this.embed(result.EmbedUrl, $videoContainer);
                } else {
                    $container.empty();
                }
            });
        },
        embed: function(id, container) {
            $container = $container || $videoContainer;

            var markup = '<iframe width="620" height="349" src="https://www.youtube.com/embed/' + id + '" frameborder="0" allowfullscreen></iframe>';
            $container.empty().append(markup);
        }
    }
}