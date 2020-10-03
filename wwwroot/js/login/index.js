LoginViewModel = function (data) {
    "use strict";

    var self = this,
        formLogin = $("#form-login"),
        buttonLogin = $("#button-login");

    self.Email = ko.observable(data.Email);
    self.Password = ko.observable(data.Password);

    self.init = function () {

        formLogin.form({
            onSuccess: function (e) {
                e.preventDefault();

                $.ajax({
                    url: "/login",
                    type: "POST",
                    data: ko.toJSON(self),
                    contentType: "application/json",
                    beforeSend: function (xhr, status) {
                        buttonLogin.addClass("loading disabled").blur();
                    },
                    success: function (json, status, xhr) {
                        if (xhr.readyState === 4 && xhr.status === 200) {
                            window.location = json.redirect;
                        }
                    },
                    complete: function (xhr) {
                        buttonLogin.removeClass("loading");
                        if (xhr.readyState === 4 && xhr.status !== 200) {
                            
                        }
                    }
                });
            }


        });
    };

    self.login = function () {
        formLogin.form("submit");
    };

    self.init();
};