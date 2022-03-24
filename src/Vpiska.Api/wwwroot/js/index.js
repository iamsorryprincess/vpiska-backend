import { getJson, postFile, removeFiles } from "./http.js";
import { setPagination, size, itemsCount, currentPage, getData, setItemsCount, totalItemsCount } from "./pagination.js";
import { createRowItem } from "./item.js";

getJson(`api/media?page=1&size=${size}`)
    .then(response => {
      if (!response.isSuccess) {
        console.log(response.errors);
        return;
      }
      setPagination(response.result);
    })
    .catch(error => console.log(error));

function sendFile() {
  postFile('api/media', this.files[0])
      .then(response => {
        if (!response.isSuccess) {
          console.log(response.errors);
          return;
        }
        
        setItemsCount(totalItemsCount + 1);

        if (itemsCount + 1 <= size) {
          const container = document.querySelector('#items-container');
          const element = createRowItem(response.result);
          container.append(element);
        }
      })
      .catch(error => console.log(error));
}

const fileInput = document.querySelector('#file');
fileInput.addEventListener('change', sendFile);

const deleteForm = document.forms[0];
const deleteButton = deleteForm.querySelector('button');
const headerCheckbox = document.querySelector('.row-header').querySelector('input');

headerCheckbox.addEventListener('change', function () {
  const itemsCheckboxes = document.querySelector('#items-container').querySelectorAll('input');
  itemsCheckboxes.forEach(function (checkbox) {
    checkbox.checked = headerCheckbox.checked;
  });
  if (itemsCheckboxes.length > 0 && headerCheckbox.checked) {
    deleteForm.classList.remove('hidden');
  } else {
    deleteForm.classList.add('hidden');
  }
});

deleteButton.addEventListener('click', function () {
  const names = [];
  const itemsCheckboxes = document.querySelector('#items-container').querySelectorAll('input');
  
  itemsCheckboxes.forEach(function (checkbox) {
    if (checkbox.checked) {
      names.push(checkbox.nextElementSibling.textContent);
    }
  });
  
  removeFiles('api/media', names)
      .then(response => {
        if (!response.isSuccess) {
          console.log(response.errors);
          return;
        }

        deleteForm.classList.add('hidden');
        getData(currentPage);
      })
      .catch(error => console.log(error));
});
