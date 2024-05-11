import { ComponentFixture } from '@angular/core/testing';

export class FixtureHelper {

  private elementToUse: any;

  constructor(
    private readonly fixture: ComponentFixture<any>,
    public readonly element: any = undefined,
  ) {
    this.elementToUse = element ?? this.fixture.nativeElement;
  }

  findElement(selector: string): FixtureHelperElement {
    const element = this.elementToUse.querySelector(selector);
    return new FixtureHelperElement(this.fixture, element);
  }

  findElements(selector: string): FixtureHelperElement[] {
    const elements = this.elementToUse.querySelectorAll(selector);
    return [...elements].map((element: any) => new FixtureHelperElement(this.fixture, element));
  }

  click(): void {
    this.elementToUse.click();
  }

  clickOn(selector: string): void {
    const button = this.findElement(selector).element as HTMLButtonElement;
    return button.click();
  }

  getTrimmedTextContent(): string {
    return this.elementToUse.textContent.trim();
  }
}

export class FixtureHelperElement extends FixtureHelper {

  constructor(
    fixture: ComponentFixture<any>,
    element: any,
  ) {
    super(fixture, element);
  }

}
