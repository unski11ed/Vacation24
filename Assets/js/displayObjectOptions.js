import { placeOptions } from './placeOptions';

export function displayObjectOptions($container, options) {
    for (var prop in options) {
        if (options.hasOwnProperty(prop) && options[prop]) {
            var option = placeOptions.findByModelName(prop);
            if (option) {
                $container.append(
                    '<span class="option_icon ' + option.iconClass + ' " rel="tipsy" title="' + option.title + '"></span>'
                );
            }
        }
    }
}
