import { Component, inject } from '@angular/core';
import { LocalizationService } from "@services/localization/localization.service";
import { TranslatePipe } from '../../pipes/translate.pipe';

@Component({
    selector: 'app-welcome-modal',
    templateUrl: './welcome-modal.component.html',
    styleUrls: ['./welcome-modal.component.sass'],
    imports: [TranslatePipe]
})
export class WelcomeModalComponent {

  private readonly localizationService = inject(LocalizationService);

  get description1() {
    return this.localizationService.translate('WELCOME_MODAL.DESCRIPTION_1', {
      guidesLink: '<span class="text-primary">guides.fivepd.com</span>',
    }, false);
  }

  get description2() {
    return this.localizationService.translate('WELCOME_MODAL.DESCRIPTION_2', {
      fivepdLink: '<span class="text-primary">fivepd.com</span>',
    }, false);
  }

  get dismiss() {
    return this.localizationService.translate('WELCOME_MODAL.DISMISS', {
      key: '<kbd class="px-1.5 py-1 font-medium bg-zinc-700 border border-zinc-800 rounded-md">F1</kbd>',
    }, false);
  }

}
