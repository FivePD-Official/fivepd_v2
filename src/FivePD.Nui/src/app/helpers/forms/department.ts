import { Department, TextInput } from '@models';
import { LocalizationService } from '@services/localization/localization.service';
import { CustomValidators } from '../validators';

export function createDepartmentCallsignFormatInputField(localizationService: LocalizationService, value?: Department) {
  return new TextInput({
    key: 'CallsignFormat',
    required: true,
    label: 'UI.LABEL.CALLSIGN.FORMAT.TITLE',
    helpLabel: `
              ${localizationService.translate('UI.LABEL.CALLSIGN.FORMAT.HELP_LABEL')} ${localizationService.translate('UI.LABEL.AVAILABLE_CHARACTERS')}: <br/>
              <table>
                <tr>
                  <td><b>L</b><td>
                  <td>(${localizationService.translate('UI.LABEL.LETTER.SINGULAR')})<td>
                </tr>
                <tr>
                  <td><b>N</b><td>
                  <td>(${localizationService.translate('UI.LABEL.NUMBER.SINGULAR')})<td>
                </tr>
                <tr>
                  <td><b>-</b><td>
                  <td>(${localizationService.translate('UI.LABEL.DASH.SINGULAR')})<td>
                </tr>
                <tr>
                  <td><b>A-Z</b><td>
                  <td>(${localizationService.translate('UI.LABEL.ANY_LETTER')})<td>
                </tr>
                <tr>
                  <td><b>0-9</b><td>
                  <td>(${localizationService.translate('UI.LABEL.ANY_NUMBER')})<td>
                </tr>
              </table>
            `,
    validators: [CustomValidators.callsignFormat()],
    value: value?.CallsignFormat,
  });
}
