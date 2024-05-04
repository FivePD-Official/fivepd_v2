import { Directive, Input } from "@angular/core";

@Directive({
  selector: '[tippy]',
})
class TippyDirective {
  @Input('tippy') content: any;
}

@Directive({
  selector: '[isEnabled]',
})
class TippyIsEnabledDirective {
  @Input('isEnabled') content: any;
}

@Directive({
  selector: '[placement]',
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
