# Envelope Manager

### Background
For awhile now, I have more or less subscribed to the ideas of the personal finance radio commentator
[Dave Ramsey](https://www.daveramsey.com/).
For those not familiar, Dave's central thesis is that with thoughtful planning, hard work, and religious avoidance of debt, almost
anyone can build wealth and elevate their financial standing. One of Dave's favorite talking points is the monthly budget, but as
a college student with a lot of irregular income and even more irregular expenses (think tuition), I really didn't like the idea of
organizing my budget by month. Much to my dismay, I discovered that apparently I'm in the minority - every website, app, and tool
I could find was oriented around the idea of monthly income and expenses. For awhile, I tried using Excel, but the system I
devised was so cumbersome that I would often let receipts accrue for weeks before entering them in, which completely defeated the
purpose of a budget. It always felt like I was trying to hammer a nail with a screwdriver. Eventually, inspired by another of Dave's
favorite techniques - the [envelope system](https://www.daveramsey.com/blog/envelope-system-explained) - I wrote this program in
the hopes that it would let me manage things a little more efficiently.

### Usage information
I think everything should be pretty self-explanatory. You add virtual "envelopes" and see their respective balances on the left-hand
side. There's no way to delete envelopes as yet - I'll probably add that functionality when I have a need for it. Once you have
envelopes, you add transactions on the right-hand side - you can deposit, withdraw, and transfer to and from any envelope.
Envelopes turn red if they dip below zero - if that happens, call Dave Ramsey to enjoy being chewed out on live radio :wink:

Your account is saved to a .CSV file of your choice. The .CSV file basically just stores a complete transaction history through
which the application fast-forwards every time you open it. This might eventually lead to lags during startup, but it makes the
code much simpler, and I suspect the history would have to get unreasonably large before this would become a performance conern.
This has the added benefit of allowing you to view up the entire transaction history in a separate window.
