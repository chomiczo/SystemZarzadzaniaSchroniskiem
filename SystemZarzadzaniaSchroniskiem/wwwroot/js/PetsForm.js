let breedList = []
let previousIndex = 0
let selectedIndex = 0

const breedQuery = document.getElementById('breed-query')
const breedInput = document.getElementById('breed-input')
const speciesInput = document.getElementById('species-input')
const breedDropdown = document.getElementById('breed-dropdown')

const setBreed = (breed) => {
  breedQuery.value = breed.name
  breedInput.value = breed.id
}

const updateSelection = () => {
  //console.log({ selectedIndex, previousIndex, len: breedList.length })
  const dropdownElements = breedDropdown.querySelectorAll('.breed-link')

  for (let i = 0; i < breedList.length; i++) {
    if (i === selectedIndex) {
      dropdownElements[i].classList.add('active')
    } else {
      dropdownElements[i].classList.remove('active')
    }
  }

  if (selectedIndex === 0 && previousIndex === breedList.length - 1) {
    breedDropdown.scroll({ top: 0 })
  } else if (selectedIndex > 3) {
    breedDropdown.scrollTo({
      top: dropdownElements[selectedIndex - 3].offsetTop,
    })
  }
}

const reloadBreeds = async () => {
  const species = speciesInput.value
  const query = breedQuery.value

  const spinner = document.createElement('li')
  spinner.classList.add('dropdown-item', 'breed-dropdown-spinner')
  spinnerIcon = document.createElement('i')
  spinnerIcon.classList.add('fas', 'fa-spinner')
  spinner.appendChild(spinnerIcon)
  breedDropdown.replaceChildren(spinner)

  const breeds = await fetch(`/api/breeds/${species}/${query}`)
  if (breeds.status === 200) {
    breedList = await breeds.json()
  }

  breedDropdown.removeChild(spinner)

  if (breedList.length === 0) {
    const placeholder = document.createElement('li')
    placeholder.classList.add('dropdown-item', 'breed-dropdown-placeholder')
    const placeholderIcon = document.createElement('i')
    placeholderIcon.classList.add('fas', 'fa-ban')
    placeholder.appendChild(placeholderIcon)
    breedDropdown.replaceChildren(placeholder)
  } else {
    selectedIndex = 0
    previousIndex = 0

    for (let i = 0; i < breedList.length; i++) {
      const breed = breedList[i]
      //console.log(breed.id, breed.name)
      const breedItem = document.createElement('li')
      const breedLink = document.createElement('a')
      breedLink.setAttribute('role', 'button')

      breedItem.appendChild(breedLink)
      breedLink.classList.add(
        'breed-link',
        'dropdown-item',
        i == selectedIndex ? 'active' : undefined,
      )
      breedLink.innerText = breed.name

      breedLink.addEventListener('click', e => {
        //console.log({ e })
        // breedQuery.value = breed.name
        setBreed(breed)
      })

      breedDropdown.appendChild(breedItem)
    }
  }
}

breedQuery.addEventListener('keypress', e => {
  // Nie wysyłaj formularza
  if (e.key === 'Enter') {
    e.preventDefault()
  }
})

breedQuery.addEventListener('keyup', async e => {
  if (!breedDropdown.classList.contains('show')) {
    breedDropdown.classList.add('show')
  }

  switch (e.key) {
    case 'Enter':
      setBreed(breedList[selectedIndex])
      breedDropdown.classList.remove('show')
      break

    case 'Escape':
      breedDropdown.classList.remove('show')
      break

    case 'ArrowUp':
      previousIndex = selectedIndex
      selectedIndex--
      if (selectedIndex < 0) {
        selectedIndex = breedList.length - 1
      }
      updateSelection()
      break

    case 'ArrowDown':
      previousIndex = selectedIndex
      selectedIndex = (selectedIndex + 1) % breedList.length
      updateSelection()
      break

    default:
      await reloadBreeds()
      break
  }
})

breedQuery.addEventListener('focus', async e => {
  breedDropdown.classList.add('show')
  await reloadBreeds()
})

breedQuery.addEventListener('blur', async e => {
  setTimeout(() => {
    breedDropdown.classList.remove('show')
  }, 100)
})

reloadBreeds()
updateSelection()
