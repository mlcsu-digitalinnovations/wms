$(document).ready(function () {
  $("#DontContactByEmail").change(function () {

    if ($('#DontContactByEmail').is(':checked')) {
      $("#Email").attr("disabled", true).addClass("disabled");
      $("#email-not-supplied-text").removeClass("hidden");
    } else {
      $("#Email").attr("disabled", false).removeClass("disabled");
      $("#email-not-supplied-text").addClass("hidden");
    }
  });

});