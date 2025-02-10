"use strict";

var RmcUi = RmcUi || {};

RmcUi.Overrides = RmcUi.Overrides || {}

RmcUi.Overrides.Exception =
  $.extend(RmcUi.Overrides.Exception || {}, function () {
    // button action
    const buttonEvent = function () {
      openDialog('wms-exception-override-dialog', this);
    }

    const cancelEvent = function () {
      closeDialog(this);
    }
    // setup button
    const button = document.getElementById('wms-exception-override-button');
    const cancelButton = document.getElementById('cancelexception-override');

    if (button != null && cancelButton != null) {
      button.addEventListener('click', buttonEvent);
      cancelButton.addEventListener('click', cancelEvent);
    }

    return { };

  }());