

$(document).ready(function () {
  document.getElementById("wms-help-bar").classList.add("cookies-visible");

  function handleCloseCookies() {
    document.getElementById('cookiebanner').style.display = 'none';
    document.getElementById("wms-help-bar").classList.remove("cookies-visible");
  }

  $("#nhsuk-cookie-banner__link_accept_analytics").off('click').on('click', function () {
    handleCloseCookies();
  });
});



