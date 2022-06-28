"use strict";

window.addEventListener("load", function () {
  var connection = new signalR.HubConnectionBuilder()
    .withUrl(window._signalRUrl)
    .build();

  connection.start(connection).then(function () {
    // clear any existing indicators for this user
    var activeUser = document.getElementById("ActiveUser").value;
    connection
      .invoke("ClearReferralActionStatus", activeUser)
      .catch(function (err) {
        return console.error(err.toString());
      });
  });

  connection.on("NewReferral", function () {
    // message broadcast when a referral is created
    if (window.location.pathname.toLowerCase().includes("referrallist")) {
      refreshPage("referralList");
    }
  });

  connection.on("NewVulnerableReferral", function () {
    // message broadcast when a vulnerable referral is created
    if (window.location.pathname.toLowerCase().includes("vulnerablelist")) {
      refreshPage("vulnerableList");
    }
  });

  connection.on("ClearReferralActionStatus", function (activeUser) {
    // message broadcast when a user enters a referral list page
		// clear any previous markers
		if(activeUser == null) {
			return;
		}
    clearIndicators(activeUser, null);
  });

  connection.on(
    "UpdateReferralActionStatus",
    function (activeUser, referralId) {
      // message broadcast when a user enters the referral view page
			// clear any previous markers

			if(activeUser == null || referralId == null) {
				return;
			}

      clearIndicators(activeUser, referralId);
      try {
        var user = JSON.parse(activeUser);

				// remove the view link
				var viewCell = document.getElementById("view-" + referralId);

				if (viewCell !== null)
				{
					var link = viewCell.getElementsByTagName('a')[0];
					link.style.display = 'none';

					if (viewCell.getElementsByClassName("nhsuk-tag").length == 0)
					{
						var span = document.createElement("span");
						span.className = "nhsuk-tag";
						span.innerHTML = user.UserInitials;
						span.setAttribute("data-user-id", user.UserIdentifier);
						viewCell.appendChild(span);
					}
				}
      } catch (ex) {
        console.log(ex);
      }
    }
  );
});

function refreshPage(pageName) {
  try {
    var xhttp = new XMLHttpRequest();

    xhttp.onreadystatechange = function () {
      if (this.readyState == 4 && this.status == 200) {
				var refreshedPage = document.open('text/html', 'replace');
				refreshedPage.write(this.responseText);
				refreshedPage.close();
      }
    };
    xhttp.open("GET", pageName, true);
    xhttp.send();
  } catch (ex) {
    console.log(ex);
  }
}

function clearIndicators(activeUser, referralId) {

	if(activeUser == null) {
		return;
	}

	try {
		var user = JSON.parse(activeUser);
		var workingCells = document.getElementsByClassName("nhsuk-tag");

		for (var i=0; i<workingCells.length; i++) {
			if (workingCells[i].getAttribute("data-user-id") == user.UserIdentifier) {

				var parent = workingCells[i].parentElement;
				parent.getElementsByClassName("nhsuk-tag")[0].remove();

				var link = parent.getElementsByTagName('a')[0];
				link.style.display = 'block';
			}
		}
	}
	catch (ex) {
		console.log(ex);
	}
}
