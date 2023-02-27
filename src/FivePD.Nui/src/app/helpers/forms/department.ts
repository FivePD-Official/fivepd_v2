import { Department, TextInput } from '@models';
import { LocalizationService } from '@services/localization/localization.service';
import { CustomValidators } from '../validators';

export function createDepartmentCallsignFormatInputField(localizationService: LocalizationService, value?: Department) {
  return new TextInput({
    key: 'CallsignFormat',
    required: true,
    label: 'Nui.Labels.CallsignFormat',
    helpLabel: `
              ${localizationService.translate('Nui.Labels.CallsignFormatHelpLabel')} ${localizationService.translate('Nui.Labels.AvailableCharacters')}: <br/>
              <table>
                <tr>
                  <td><b>L</b><td>
                  <td>(${localizationService.translate('Nui.Labels.Letter')})<td>
                </tr>
                <tr>
                  <td><b>N</b><td>
                  <td>(${localizationService.translate('Nui.Labels.Number')})<td>
                </tr>
                <tr>
                  <td><b>-</b><td>
                  <td>(${localizationService.translate('Nui.Labels.Dash')})<td>
                </tr>
              </table>
            `,
    validators: [CustomValidators.callsignFormat],
    value: value?.CallsignFormat,
  });
}
