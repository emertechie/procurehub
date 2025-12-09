
* How to wrap creation of user and staff in a txn?
* Add test
  * Can I add a test that asserts ASP.Net user won't be created if Staff can't be?
* Refactor ICommand APIs ets to match MediatR. And get AI to generate same scanning functionality
  * CancellationToken: public async Task<int> Handle(Command message, CancellationToken token)
* Staff could be in multiple departments
