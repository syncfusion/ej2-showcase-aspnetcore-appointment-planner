var plannerPage;
(function (page) {
    plannerPage[page["dashboard"] = 0] = "dashboard";
    plannerPage[page["calendar"] = 1] = "calendar";
    plannerPage[page["doctors"] = 2] = "doctors";
    plannerPage[page["patients"] = 3] = "patients";
    plannerPage[page["preference"] = 4] = "preference";
    plannerPage[page["about"] = 5] = "about";


})(plannerPage || (plannerPage = {}));

var currentPage;
function getCurrentPage() {
    switch (window.location.hash) {
        case ('#/' + plannerPage[plannerPage.dashboard]):
            currentPage = plannerPage[plannerPage.dashboard];
            break;
        case ('#/' + plannerPage[plannerPage.calendar]):
            currentPage = plannerPage[plannerPage.calendar];
            break;
        case ('#/' + plannerPage[plannerPage.doctors]):
            currentPage = plannerPage[plannerPage.doctors];
            break;
        case ('#/' + plannerPage[plannerPage.patients]):
            currentPage = plannerPage[plannerPage.patients];
            break;
        case ('#/' + plannerPage[plannerPage.preference]):
            currentPage = plannerPage[plannerPage.preference];
            break;
        case ('#/' + plannerPage[plannerPage.about]):
            currentPage = plannerPage[plannerPage.about];
            break;
    }
    return currentPage;
}

routeDefault();

plannerSideBar();



crossroads.addRoute('/:lang:', renderPartialContent).rules = { lang: ['dashboard', 'calendar', 'doctors', 'patients', 'preference', 'about'] };
crossroads.bypassed.add(function (request) {
    var samplePath = ['dashboard', 'calendar', 'doctors', 'patients', 'preference', 'about', 'doctor-details'];
    var hash = request.split(' ')[0];
    var isDoctorDetail = hash.includes(samplePath[samplePath.length - 1]);
    if (samplePath.indexOf(hash) === -1 && !isDoctorDetail) {
        location.hash = '#/' + samplePath[0];
    } else if (isDoctorDetail) {
        renderPartialContent(request);
    }
});

function renderPartialContent(requestPage) {
    var isDoctorDetailPage = requestPage.includes('doctor-details');
    if ((currentPage && currentPage !== '') || (window.location.hash === '#/' + getCurrentPage()) || isDoctorDetailPage) {
        var dialogEle = document.querySelectorAll('body .e-dialog');
        if (dialogEle.length > 0) {
            dialogEle.forEach(ele => ele.ej2_instances[0].destroy());
        }
        var sidebar = document.querySelector('#plannerSiderBar');
        var activeItem = sidebar.querySelectorAll('.active-item');
        if (activeItem.length === 0 || !(activeItem[0].classList.contains('doctors') && currentPage.includes('doctor'))) {
            activeItem.forEach(element => {
                if (element.classList.contains('active-item')) { element.classList.remove('active-item'); }
            });
        }
        if (isDoctorDetailPage) {
            var doctorId = window.location.hash.split('/')[2];
            var ajaxHTML = new ej.base.Ajax('Home/DoctorDetails', 'POST', true);
            currentPage = 'doctors';
            crossroads.__proto__.ignoreState = true;
            ajaxHTML.send(JSON.stringify(doctorId)).then(function (value) {
                appendContent(value);
            });
        } else {
            var ajaxHTML = new ej.base.Ajax('Home/' + currentPage.charAt(0).toUpperCase() + currentPage.slice(1), 'GET', true);
            ajaxHTML.send().then(function (value) {
                appendContent(value);
            });
        }
    }
}

function appendContent(value) {
    document.getElementById('content').innerHTML = '';
    document.getElementById('content').innerHTML = value.toString();
    if (!document.querySelector('.sb-content-overlay').classList.contains('sb-hide')) {
        document.querySelector('.sb-content-overlay').classList.add('sb-hide');
    }
    document.querySelector('.planner-wrapper').style.visibility = 'visible';
    document.querySelector('.planner-wrapper').style.opacity = '1';
    var sidebar = document.querySelector('#plannerSiderBar');
    if (ej.base.Browser.isDevice) {
        sidebar.ej2_instances[0].hide();
    }
    ej.base.addClass([sidebar.querySelector('.sidebar-item.' + currentPage)], 'active-item');
    renderControl("content");
}

function renderControl(id) {
    var scripts = document.querySelectorAll("#" + id + " script");
    var length = scripts.length;
    for (var i = 0; i < length; i++) {
        if (scripts[i].id == "")
            eval(scripts[i].text);
    }
}

function routeDefault() {
    crossroads.addRoute('', function () {
        window.location.href = window.location.href.lastIndexOf("/") === window.location.href.length - '/'.length ? '#/dashboard' : window.location.href + "/#/dashboard";
    });
}

function plannerSideBar() {
    document.body.classList.add('main-page');
    var sidebar = document.querySelector('#plannerSiderBar').ej2_instances[0];
    var isDevice = ej.base.Browser.isDevice;
    sidebar.showBackdrop = isDevice;
    sidebar.closeOnDocumentClick = isDevice;
    if (isDevice) {
        document.querySelector('.planner-header').classList.add('device-header');
        document.querySelector('.planner-wrapper').classList.add('device-wrapper');
        document.querySelector('.planner-header .open-icon.e-icons').onclick = function () {
            var sidebarObj = document.querySelector('#plannerSiderBar').ej2_instances[0];
            sidebarObj.show();
        };
    }
}

hasher.initialized.add(function (hashValue) {
    crossroads.parse(hashValue);
});

hasher.changed.add(function (hashValue) {
    currentPage = hashValue;
    crossroads.parse(hashValue);
});
hasher.init();

function destroyErrorElement(formElement, inputElements) {
    if(formElement) {
        var elements = [].slice.call(formElement.querySelectorAll('.field-error'));
        for (var elem of elements) {
            ej.base.remove(elem);
        }
        for (var element of inputElements) {
            if (element.querySelector('input').classList.contains('e-error')) {
                ej.base.removeClass([element.querySelector('input')], 'e-error');
            }
        }
    }
}

function renderFormValidator(formElement, rules, parentElement) {
    var model = {
        customPlacement: (inputElement, error) => { errorPlacement(inputElement, error); },
        rules: rules,
        validationComplete: (args) => {
            validationComplete(args, parentElement);
        }
    };
    var obj = new ej.inputs.FormValidator(formElement, model);
}

function validationComplete(args, parentElement) {
    var elem = parentElement.querySelector('#' + args.inputName + '_Error');
    if(elem) {
        elem.style.display = (args.status === 'failure') ? '' : 'none';
    }
}

function errorPlacement(inputElement, error) {
    var id = error.getAttribute('for');
    var elem = inputElement.parentElement.querySelector('#' + id + '_Error');
    if(!elem) {
        var div = ej.base.createElement('div', {
            className: 'field-error',
            id: inputElement.getAttribute('name') + '_Error'
        });
        var content = ej.base.createElement('div', { className: 'error-content' });
        content.appendChild(error);
        div.appendChild(content);
        inputElement.parentElement.parentElement.appendChild(div);
    }
}

function updateDoctorDetail(value) {
    var dialogEle = document.querySelectorAll('body .e-dialog');
    if (dialogEle.length > 0) {
        dialogEle.forEach(ele => ele.ej2_instances[0].destroy());
    }
    document.getElementById('content').innerHTML = '';
    document.getElementById('content').innerHTML = value.toString();
    renderControl("content");
}

window.addEventListener('beforeunload', function (e) {
    fetch('Home/ResetService', {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        }
    });
});