import { Directive, Input } from "@angular/core";

@Directive({
    selector: '[tippy]',
    standalone: false
})
class TippyDirective {
  @Input('tippy') content: any;
}

@Directive({
    selector: '[isEnabled]',
    standalone: false
})
class TippyIsEnabledDirective {
  @Input('isEnabled') content: any;
}

@Directive({
    selector: '[placement]',
    standalone: false
})
class TippyPlacementDirective {
  @Input('placement') content: any;
}

export function mockedTippyModule() {
  return {
    set: {
      declarations: [TippyDirective, TippyIsEnabledDirective, TippyPlacementDirective],
      exports: [TippyDirective, TippyIsEnabledDirective, TippyPlacementDirective]
    }
  };
}
