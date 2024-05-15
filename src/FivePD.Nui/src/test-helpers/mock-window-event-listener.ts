export class MockWindowEventListener {

  events: Record<string, any> = {};

  constructor() {
    window.addEventListener = jest.fn().mockImplementation((event, cb) => {
      this.events[event] = cb;
    });
  }

  keydown(options: { code: string }): void {
    const event = this.events['keydown'];
    if (event) {
      event(options);
    }
  }

  message(options: { data: any }): void {
    const event = this.events['message'];
    if (event) {
      event(options);
    }
  }
}
