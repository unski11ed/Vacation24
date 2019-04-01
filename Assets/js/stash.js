import $ from 'jquery';
import EventEmitter from 'events';

import { notification } from './notification';
import { ajaxRequest } from './ajaxRequest';

var STASH_LIMIT = 10;

function stashInitialize() {
    var _this = this;

    var list = [];
    var onStashChanged = new EventEmitter();

    function save() {
        window.localStorage.setItem("stash", JSON.stringify(list));
    }

    function load() {
        list = JSON.parse(window.localStorage.getItem("stash"));
        
        return list || [];
    }

    load();

    return {
        onStashChanged: onStashChanged,
        add: function (id) {
            if (list.length >= STASH_LIMIT) {
                notification.notice("Stash is full. Remove some items to add new ones.");
            }
    
            // Prevent duplicates
            if (list.filter(function (element) { return element.Id == id }).length) {
                return;
            }
    
            ajaxRequest("/Stash/Get", {
                Id: id
            }, function (data) {
                if (data.status === ResultType.Success) {
                    list.push(data.item);
                    save();
                    onStashChanged.emit(list);
                }
            });
        },
        list: function() {
            return list;
        },
        remove: function(id) {
            for (var i = 0; i < list.length; i++) {
                var element = list[i];
                if (element.Id == id) {
                    list.splice(i, 1);
                    save();
                    onStashChanged.emit(list);
                    break;
                }
            }
        },
        getCount: function() {
            return list.length;
        }
    }
}