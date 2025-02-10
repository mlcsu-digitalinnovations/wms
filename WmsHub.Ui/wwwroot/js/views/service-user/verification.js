
$(document).ready(function () {

  let maxYear = new Date().getFullYear();

  $("#Year").attr("data-val-range", `Year must be between 1900 and ${maxYear}`);
  $("#Year").attr("data-val-range-min", 1900);
  $("#Year").attr("data-val-range-max", maxYear);

  $("#confirm-button").click(function (e) {
    let year = parseInt($("#Year").val());
    let month = parseInt($("#Month").val());
    let day = parseInt($("#Day").val());

    if (year && month && day) {
      month -= 1; 
      let date = new Date(year, month, day);

      if (date.getFullYear() !== year
        || date.getMonth() !== month
        || date.getDate() !== day) {
        $("#invalid-date-error").removeClass("nhsuk-u-visually-hidden");
        e.preventDefault();
      }
      else {
        $("#invalid-date-error").addClass("nhsuk-u-visually-hidden");
      }
    }        
  });
});