function limitChanged() {
  var limit = document.getElementById("limit-select").value;
  document.getElementById("Search_Limit").value = limit;

  $('#referralSearch').submit();
}

$(document).ready(function () {
  var existingLimit = document.getElementById("Search_Limit");

  if (existingLimit.value == null || existingLimit.value == '') {
    existingLimit.value = 25;
  } else {
    $('#limit-select').val(existingLimit.value);
  }

  $('#limit-select').off('change').on('change', function () {
    limitChanged();
  });
});