import $ from 'jquery';

import { ajaxRequest } from './ajaxRequest';
import { notification } from './notification';

export function contactFormInitialize($form, objectId) {
    var _this = this;

    $form.submit(function (e) {
        e.preventDefault();

        var content = $(this).find("textarea[name=Content]").val(),
            subject = $(this).find("input[name=Subject]").val(),
            email = $(this).find("input[name=Email]").val();

        if (
            content.length >= 3 &&
            subject.length >= 3
        ) {
            _this.Send(subject, content, email, objectId);

            $(this).find("input[type=submit]").attr('disabled', true);
        }

        return false;
    });

    return {
        send: function (subject, content, email, id) {
            ajaxRequest("/Messages/Send", {
                Subject: subject,
                Content: content,
                ObjectId: id,
                Email: email
            }, function (data) {
                if (data.Status == ResultType.Success) {
                    notification.success(data.Message);
    
                    $form.find('textarea,input[type=text],input[type=email]').val('');
                }
            }, function () {
                notification.error("Nie udało się wysłać wiadomości, spróbuj jeszcze raz.");
            });
        }
    }
}