'use strict';

var connection;

window.addEventListener('load', () => {
    this.connection = new signalR.HubConnectionBuilder().withUrl(window._signalRUrl).build();

    connection.start().then(function () {
        notifyHub();
    });
})

const notifyHub = () => {
    try {
				var activeUser = document.getElementById('ActiveUser').value;
				var referralId = document.getElementById('Id').value;

				this.connection.invoke('UpdateReferralActionStatus',	activeUser,	referralId)
				.catch(function (err) {
            return console.error(err.toString());
        });

        setTimeout(() => {
            notifyHub()
        }, 5000);

    }
    catch (ex)
    {
        console.log(ex)
    }
}

window.addEventListener("beforeunload", function () {

	var activeUser = document.getElementById('ActiveUser').value;
	this.connection
      .invoke("ClearReferralActionStatus", activeUser)
      .catch(function (err) {
        return console.error(err.toString());
      });
})
