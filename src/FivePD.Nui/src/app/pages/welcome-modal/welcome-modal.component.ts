import { Component } from '@angular/core';
import { LocalizationService } from "@services/localization/localization.service";

@Component({
  selector: 'app-welcome-modal',
  templateUrl: './welcome-modal.component.html',
  styleUrls: ['./welcome-modal.component.sass']
})
export class WelcomeModalComponent {

  constructor(private readonly _localizationService: LocalizationService) {
  }


  get description1() {
    return this._localizationService.translate('WELCOME_MODAL.DESCRIPTION_1', {
      guidesLink: '<span class="text-primary">guides.fivepd.com</span>',
    }, false);
  }

  get description2() {
    return this._localizationService.translate('WELCOME_MODAL.DESCRIPTION_2', {
      fivepdLink: '<span class="text-primary">fivepd.com</span>',
    }, false);
  }

}
