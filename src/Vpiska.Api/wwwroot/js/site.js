const uploadForm = document.forms[1];
const input = uploadForm.querySelector('input');

input.addEventListener('change', function () {
  uploadForm.submit();
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

const deleteForm = document.forms[0];
const rowItems = document.querySelectorAll('.row-item');
const rowHeaderCheckbox = document.querySelector('.row-header').querySelector('input');

function createInput(value) {
  const input = document.createElement('input');
  input.name = 'names';
  input.value = value;
  input.setAttribute('hidden', 'true');
  return input;
}

function findInput(value) {
  for (let i = 0; i < deleteForm.children.length; i++) {
    if (deleteForm.children[i].value === value) {
      return deleteForm.children[i];
    }
  }
  
  return null;
}

let checkedCount = 0;

function toggleRemoveButton() {
  if (checkedCount === 0) {
    deleteForm.classList.add('hidden');
  } else {
    deleteForm.classList.remove('hidden');
  }
}

function setChecked(checked) {
  if (!checked && checkedCount === 0) {
    return;
  }
  rowItems.forEach(function (rowItem) {
    rowItem.querySelector('input').checked = checked;
    const name = rowItem.querySelector('span').textContent;
    if (checked) {
      deleteForm.appendChild(createInput(name));
      checkedCount++;
    } else {
      deleteForm.removeChild(findInput(name));
      checkedCount--;
    }
  });
  toggleRemoveButton();
}

rowHeaderCheckbox.addEventListener('change', function () {
  setChecked(rowHeaderCheckbox.checked);
});

rowItems.forEach(function (rowItem) {
  rowItem.querySelector('input').addEventListener('change', function () {
    const name = rowItem.querySelector('span').textContent;
    if (rowItem.querySelector('input').checked) {
      deleteForm.appendChild(createInput(name));
      checkedCount++;
    } else {
      deleteForm.removeChild(findInput(name));
      checkedCount--;
    }
    toggleRemoveButton();
  });
  
  const itemImg = rowItem.parentElement.querySelector('.img-container');
  
  rowItem.addEventListener('click', function () {
    itemImg.toggleAttribute('hidden');
  });
});