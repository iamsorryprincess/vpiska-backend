const form = document.forms[0];
const input = form.querySelector('input');

input.addEventListener('change', () => {
  form.submit();
  form.reset();
});

const pager = document.querySelector('.pager');
const page = pager.attributes.getNamedItem('data-page').value;
const total = pager.attributes.getNamedItem('data-total').value;
const backLink = document.getElementById('back');
const nextLink = document.getElementById('next');

if (page - 1 === 0) {
  backLink.classList.add('disabled');
}

if (page + 1 > total) {
  nextLink.classList.add('disabled');
}
