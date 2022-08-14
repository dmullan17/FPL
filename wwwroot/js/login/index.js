LoginViewModel = function (data) {
    "use strict";

    var self = this,
        buttonLogin = $("#button-login");

    self.Email = ko.observable();
    self.Password = ko.observable();
    self.TeamId = ko.observable();

    self.init = function () {

        $(".ui.team-id.form").form({
            fields: {
                teamId: {
                    identifier: 'teamid',
                    rules: [
                        {
                            type: 'empty',
                            prompt: 'Please enter your Team ID'
                        }
                    ]
                }
            },
            onSuccess: function (e) {
                e.preventDefault();

                $("#button-set-team-id").addClass("loading");

                //TODO check if team id links to valid team before setting cookie

                document.cookie = "teamId=" + self.TeamId();
                window.location.href = window.location.origin + "/Points?entry=" + self.TeamId();
            }
        });

        //$(".ui.form").form({
        //    fields: {
        //        email: {
        //            identifier: 'email',
        //            rules: [
        //                {
        //                    type: 'empty',
        //                    prompt: 'Please enter your e-mail'
        //                },
        //                {
        //                    type: 'email',
        //                    prompt: 'Please enter a valid e-mail'
        //                }
        //            ]
        //        },
        //        password: {
        //            identifier: 'password',
        //            rules: [
        //                {
        //                    type: 'empty',
        //                    prompt: 'Please enter your password'
        //                }
        //            ]
        //        }
        //    },
        //    onSuccess: function (e) {
        //        e.preventDefault();

        //        $.ajax({
        //            url: "/login",
        //            type: "POST",
        //            data: ko.toJSON(self),
        //            contentType: "application/json",
        //            beforeSend: function (xhr, status) {
        //                buttonLogin.addClass("loading disabled").blur();
        //            },
        //            success: function (json, status, xhr) {
        //                if (xhr.readyState === 4 && xhr.status === 200) {
        //                    window.location = json.Redirect;
        //                }
        //            },
        //            complete: function (xhr) {
        //                buttonLogin.removeClass("loading");
        //                if (xhr.readyState === 4 && xhr.status !== 200) {
        //                    buttonLogin.removeClass("disabled");
        //                    $(".field").addClass("error");
                            
        //                }
        //            }
        //        });
        //    }


        //});
    };

    self.login = function () {
        formLogin.form("submit");
    };

    self.init();
};