
async function createUser() {
  const name = document.getElementById('user-name').value.trim();
  const resEl = document.getElementById('user-create-result');
  resEl.textContent = '...';
  try {
    const res = await fetch('/api/users', {
      method: 'POST',
      headers: {'Content-Type': 'application/json'},
      body: JSON.stringify({Name: name })
    });
    const data = await res.json();
    if (!res.ok) throw new Error(JSON.stringify(data));
    resEl.textContent = `User created: ${data.name} (Id: ${data.id})`;
    document.getElementById('loan-user-id').value = data.id;
    document.getElementById('query-user-id').value = data.id;
  } catch (e) {
    resEl.textContent = 'Error: ' + e.message;
  }
}

async function createLoan() {
  const userId = document.getElementById('loan-user-id').value.trim();
  const amount = parseFloat(document.getElementById('loan-amount').value);
  const annualInterestRate = parseFloat(document.getElementById('loan-rate').value);
  const loanTermYears = parseInt(document.getElementById('loan-term').value, 10);
  const paymentFrequency = document.getElementById('loan-frequency').value;

  const resEl = document.getElementById('loan-create-result');
  resEl.textContent = '...';
  try {
    const res = await fetch(`/api/users/${userId}/loans`, {
      method: 'POST',
      headers: {'Content-Type': 'application/json'},
      body: JSON.stringify({
        amount, annualInterestRate, loanTermYears, paymentFrequency
      })
    });
    const data = await res.json();
    if (!res.ok) throw new Error(JSON.stringify(data));
    resEl.textContent = `Loan created: Id ${data.id}, Amount $${data.amount}`;
    document.getElementById('schedule-loan-id').value = data.id;
  } catch (e) {
    resEl.textContent = 'Error: ' + e.message;
  }
}

async function getUserLoans() {
  const userId = document.getElementById('query-user-id').value.trim();
  const target = document.getElementById('user-loans');
  target.innerHTML = '...';
  try {
    const res = await fetch(`/api/users/${userId}/loans`);
    const loans = await res.json();
    if (!Array.isArray(loans)) throw new Error(JSON.stringify(loans));

    let html = '<table><thead><tr>' +
      '<th class="left">Loan Id</th><th>Amount</th><th>Rate %</th><th>Term (yrs)</th><th class="left">Freq</th></tr></thead><tbody>';
    for (const l of loans) {
      html += `<tr>
        <td class="left">${l.id}</td>
        <td>$${l.amount.toFixed(2)}</td>
        <td>${l.annualInterestRate.toFixed(2)}</td>
        <td>${l.loanTermYears}</td>
        <td class="left">${l.paymentFrequency}</td>
      </tr>`;
    }
    html += '</tbody></table>';
    target.innerHTML = html;
  } catch (e) {
    target.textContent = 'Error: ' + e.message;
  }
}

async function getSchedule() {
  const loanId = document.getElementById('schedule-loan-id').value.trim();
  const target = document.getElementById('schedule');
  target.innerHTML = '...';
  try {
    const res = await fetch(`/api/loans/${loanId}/schedule`);
    const rows = await res.json();
    if (!Array.isArray(rows)) throw new Error(JSON.stringify(rows));

    let html = '<table><thead><tr><th>Month</th><th>Payment</th><th>Remaining Balance</th></tr></thead><tbody>';
    for (const r of rows) {
      html += `<tr><td>${r.month}</td><td>$${r.monthlyPayment.toFixed(2)}</td><td>$${r.remainingBalance.toFixed(2)}</td></tr>`;
    }
    html += '</tbody></table>';
    target.innerHTML = html;
  } catch (e) {
    target.textContent = 'Error: ' + e.message;
  }
}

async function getSummary() {
  const loanId = document.getElementById('schedule-loan-id').value.trim();
  const month = parseInt(document.getElementById('summary-month').value, 10);
  const pre = document.getElementById('summary');
  pre.textContent = '...';
  try {
    const res = await fetch(`/api/loans/${loanId}/summary?month=${month}`);
    const data = await res.json();
    if (!res.ok) throw new Error(JSON.stringify(data));
    pre.textContent = JSON.stringify(data, null, 2);
  } catch (e) {
    pre.textContent = 'Error: ' + e.message;
  }
}
