$(document).ready(function () {
  $('#save-button').click(function () {
    const provider1IsSelected = $('#select-provider-0').is(':checked');
    const provider2IsSelected = $('#select-provider-1').is(':checked');
    const provider3IsSelected = $('#select-provider-2').is(':checked');

    if (provider1IsSelected || provider2IsSelected || provider3IsSelected) {
      $('form').submit();
    }
    else {
      displayValidationError();
    }
  });

  const displayError = $('#DisplayError').val()
  if (displayError === 'True') {
    displayValidationError();
  }

  function displayValidationError() {
    $('#error-summary').removeClass('hidden');
    $('#select-provider-form-group').addClass('nhsuk-form-group--error');
    $('#select-provider-prompt').addClass('nhsuk-error-message');
  }
});
