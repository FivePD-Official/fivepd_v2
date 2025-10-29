import { Department, TextField } from '@models';
import { LocalizationService } from '@services/localization/localization.service';
import { CustomValidators } from '../validators';

// do not use
export function createDepartmentCallsignFormatInputField(localizationService: LocalizationService, value?: Department) {
  return new TextField({
    key: 'CallsignFormat',
    required: true,
    label: 'LABEL.CALLSIGN.FORMAT.TITLE',
    helpLabel: `
              ${localizationService.translate('LABEL.CALLSIGN.FORMAT.HELP_LABEL')} ${localizationService.translate('LABEL.AVAILABLE_CHARACTERS')}: <br/>
              <table>
                <tr>
                  <td><b>L</b><td>
                  <td>(${localizationService.translate('LABEL.LETTER.SINGULAR')})<td>
                </tr>
                <tr>
                  <td><b>N</b><td>
                  <td>(${localizationService.translate('LABEL.NUMBER.SINGULAR')})<td>
                </tr>
                <tr>
                  <td><b>-</b><td>
                  <td>(${localizationService.translate('LABEL.DASH.SINGULAR')})<td>
                </tr>
                <tr>
                  <td><b>A-Z</b><td>
                  <td>(${localizationService.translate('LABEL.ANY_LETTER')})<td>
                </tr>
                <tr>
                  <td><b>0-9</b><td>
                  <td>(${localizationService.translate('LABEL.ANY_NUMBER')})<td>
                </tr>
              </table>
            `,
    validators: [CustomValidators.callsignFormat()],
    value: '',
  });
}
