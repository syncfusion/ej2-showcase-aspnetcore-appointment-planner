using AppointmentPlanner.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Syncfusion.EJ2.Schedule;

namespace AppointmentPlanner.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppointmentService service;

        public HomeController(AppointmentService appointmentService)
        {
            service = appointmentService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public PartialViewResult DashBoard()
        {
            DateTime startDate = service.StartDate;
            List<Patient> patients = service.Patients;
            ViewBag.AvailableDoctors = service.Doctors;
            ViewBag.Activities = service.Activities;
            ViewBag.SpecializationsData = service.Specializations;
            ViewBag.FirstDayOfWeek = service.GetWeekFirstDate(startDate);
            ViewBag.CurrentDayEvents = service.GetFilteredData(startDate, startDate.AddDays(1));
            ViewBag.CurrentViewEvents = service.GetFilteredData(ViewBag.FirstDayOfWeek, ViewBag.FirstDayOfWeek.AddDays(6));
            ViewBag.GridData = GetAppoinment((List<Hospital>)ViewBag.CurrentDayEvents);
            List<Hospital> diabetologyData = (ViewBag.CurrentViewEvents as List<Hospital>).Where(item => item.DepartmentId == 5).ToList();
            List<Hospital> orthopaedicsData = (ViewBag.CurrentViewEvents as List<Hospital>).Where(item => item.DepartmentId == 4).ToList();
            List<Hospital> cardiologyData = (ViewBag.CurrentViewEvents as List<Hospital>).Where(item => item.DepartmentId == 6).ToList();
            ViewBag.ChartData = service.GetAllChartPoints(diabetologyData, ViewBag.FirstDayOfWeek);
            ViewBag.ChartData1 = service.GetAllChartPoints(orthopaedicsData, ViewBag.FirstDayOfWeek);
            ViewBag.ChartData2 = service.GetAllChartPoints(cardiologyData, ViewBag.FirstDayOfWeek);
            return PartialView("DashBoard/DashBoard");
        }

        private List<Appointment> GetAppoinment(List<Hospital> currentDayEvents)
        {
            List<Appointment> appointments = new List<Appointment>();
            foreach (Hospital eventData in currentDayEvents)
            {
                Patient? filteredPatients = service.Patients.FirstOrDefault(item => item.Id.Equals(eventData.PatientId));
                Doctor? filteredDoctors = service.Doctors.FirstOrDefault(item => item.Id.Equals(eventData.DoctorId));
                if (filteredPatients != null && filteredDoctors != null)
                {
                    appointments.Add(new Appointment(service.GetFormatDate(eventData.StartTime, "hh:mm tt"), filteredPatients.Name, filteredDoctors.Name, eventData.Symptoms, filteredDoctors.Id));
                }
            }
            return appointments;
        }

        [HttpGet]
        public PartialViewResult Calendar()
        {
            ViewBag.SelectedDate = service.StartDate;
            ViewBag.StartHour = service.CalendarSettings.Calendar.Start;
            ViewBag.EndHour = service.CalendarSettings.Calendar.End;
            ViewBag.WorkDays = new int[] { 0, 1, 2, 3, 4, 5, 6};
            ViewBag.CurrentView = (View)Enum.Parse(typeof(View), service.CalendarSettings.CurrentView);
            ViewBag.FirstDayOfWeek = service.CalendarSettings.FirstDayOfWeek;
            ViewBag.EventData = service.Hospitals;
            ViewBag.BookingColor = service.CalendarSettings.BookingColor;
            ViewBag.ResourceDataSource = service.Doctors;
            ViewBag.SpecialistCategory = service.Specializations;
            ViewBag.WaitingList = service.WaitingLists;
            ViewBag.PatientsData = service.Patients;
            ViewBag.Interval = service.CalendarSettings.Interval;
            return PartialView("Calendar/Calendar");
        }

        [HttpGet]
        public PartialViewResult Doctors()
        {
            ViewBag.SpecializationData = service.Specializations;
            ViewBag.Doctors = service.Doctors;
            ViewBag.FilteredDoctors = service.FilteredDoctors ?? service.Doctors;
            return PartialView("Doctor/Doctors");
        }

        [HttpGet]
        public PartialViewResult Patients()
        {
            ViewBag.FilteredPatients = service.Patients;
            ViewBag.HospitalData = service.Hospitals;
            ViewBag.Doctors = service.Doctors;
            return PartialView("Patient/Patients");
        }

        [HttpGet]
        public PartialViewResult Preference()
        {
            ViewBag.SelectedView = service.CalendarSettings.CurrentView;
            ViewBag.Views = service.Views;
            ViewBag.SelectedStartHour = service.CalendarSettings.Calendar.Start;
            ViewBag.StartHours = service.StartHours;
            ViewBag.SelectedEndHour = service.CalendarSettings.Calendar.End;
            ViewBag.EndHours = service.EndHours;
            ViewBag.TimeSlots = service.TimeSlot;
            ViewBag.TimeInterval = service.CalendarSettings.Interval;
            ViewBag.ColorCategory = service.ColorCategory;
            ViewBag.SelectedCategory = service.CalendarSettings.BookingColor;
            ViewBag.DayOfWeeks = service.DayOfWeekList;
            ViewBag.SelectedDayOfWeek = service.CalendarSettings.FirstDayOfWeek;
            return PartialView("Preference/Preference");
        }

        [HttpGet]
        public PartialViewResult About()
        {
            return PartialView("About/About");
        }

        [HttpPost]
        public PartialViewResult DoctorDetails([FromBody] string id)
        {
            int doctorId = Convert.ToInt32(id);
            Doctor activeData = service.GetDoctorDetails(doctorId);
            activeData = activeData ?? service.GetDoctorDetails(1);
            if (activeData == null)
            {
                return PartialView(string.Empty);
            }
            service.ActiveDoctors = activeData;
            activeData.WorkDays = activeData.WorkDays ?? new List<WorkDay>();
            ViewBag.GetSpecializationText = service.GetSpecializationText(activeData.Specialization);
            ViewBag.GetAvailability = service.GetAvailability(activeData);
            ViewBag.SpecializationData = service.Specializations;
            ViewBag.ExperienceData = service.Experience;
            ViewBag.DutyTimingsData = service.DutyTimings;
            ViewBag.ActiveData = activeData;
            service.FilteredDoctors = null;
            return PartialView("Doctor/DoctorDetails");
        }

        [HttpPost]
        public IActionResult UpdatePreference([FromBody] Params param)
        {
            service.UpdatePreference(param);
            return Ok();
        }

        [HttpPost]
        public PartialViewResult FilterDoctors([FromBody] Params param)
        {
            ViewBag.SpecializationValue = !string.IsNullOrEmpty(param.Value) ? param.Value : null;
            service.UpdateDoctors(param);
            return PartialView("Doctor/DoctorsList", Doctors());
        }

        [HttpPost]

        public PartialViewResult UpdateDoctors([FromBody] Doctor doctor)
        {
            UpdateCalendarDoctors(doctor);
            return PartialView("Doctor/DoctorsList", Doctors());
        }

        [HttpPost]
        public PartialViewResult RefreshDoctorsDialog()
        {
            return PartialView("Calendar/SpecialistDialogContent", Doctors());
        }

        [HttpPost]
        public PartialViewResult RefreshWaitingDialog([FromBody] string[] activeIds)
        {
            UpdateWaitingListData(activeIds);
            return PartialView("Calendar/WaitingListDialogContent");
        }

        [HttpPost]
        public PartialViewResult UpdateDoctorDetail([FromBody] Doctor doctor)
        {
            UpdateCalendarDoctors(doctor);
            return PartialView("Doctor/DoctorDetails", DoctorDetails(doctor.Id.ToString()));
        }

        [HttpPost]
        public IActionResult UpdateBreakHours([FromBody] List<WorkDay> workdays)
        {
            service.ActiveDoctors.WorkDays = workdays;
			return PartialView("Doctor/DoctorDetails", DoctorDetails(service.ActiveDoctors.Id.ToString()));
        }

        [HttpPost]
        public PartialViewResult DeleteDoctorDetail([FromBody] Params param)
        {
			if (!string.IsNullOrEmpty(param.Value))
			{
				service.Doctors = service.Doctors.Where(item => item.Id.ToString() != param.Value.ToString()).ToList();
			}
			return PartialView("Doctor/DoctorDetails", DoctorDetails(service.Doctors.FirstOrDefault()?.Id.ToString()));
		}

        [HttpPost]
        public IActionResult UpdateCalendarDoctors([FromBody] Doctor doctor)
        {
            if (doctor != null)
            {
                string dialogState = string.Empty;
                if (doctor.Id == 0)
                {
                    dialogState = "new";
                    doctor.Id = service.Doctors.Max(item => item.Id) + 1;
                    doctor.Text = "default";
                    doctor.Availability = "available";
                    doctor.Color = "#7575ff";
                    doctor.NewDoctorClass = "new-doctor";
                    doctor.AvailableDays = service.Doctors[0].AvailableDays;
                    doctor.WorkDays = service.Doctors[0].WorkDays;
                    UpdateWorkHours(doctor);
                    service.Doctors.Add(doctor);
                }
                else
                {
                    UpdateWorkHours(doctor);
                    service.ActiveDoctors = service.Doctors.First(x => x.Id == doctor.Id);
                    service.ActiveDoctors.Name = doctor.Name;
                    service.ActiveDoctors.Gender = doctor.Gender;
                    service.ActiveDoctors.Mobile = doctor.Mobile;
                    service.ActiveDoctors.Email = doctor.Email;
                    service.ActiveDoctors.Specialization = doctor.Specialization;
                    service.ActiveDoctors.Experience = doctor.Experience;
                    service.ActiveDoctors.Education = doctor.Education;
                    service.ActiveDoctors.Designation = doctor.Designation;
                    service.ActiveDoctors.DutyTiming = doctor.DutyTiming;
                    doctor = service.ActiveDoctors;
                }
                Activity activity = new()
                {
                    Name = dialogState == "new" ? "Added New Doctor" : "Updated Doctor",
                    Message = "Dr." + doctor.Name + ", " + char.ToUpperInvariant(doctor.Specialization[0]) + doctor.Specialization.Substring(1),
                    Time = "10 mins ago",
                    Type = "doctor",
                    ActivityTime = DateTime.Now
                };
                service.Activities.Insert(0, activity);
            }
            if (service.FilteredDoctors != null && service.FilteredDoctors.Count != service.Doctors.Count)
            {
                service.FilteredDoctors = service.Doctors.Where(item => item.DepartmentId.Equals(service.FilteredDoctors.First().DepartmentId)).ToList();
            }
            return Ok(doctor);
        }

        private void UpdateWorkHours(Doctor data)
        {
            string dutyString = service.DutyTimings.Where(item => item.Id.Equals(data.DutyTiming)).FirstOrDefault().Text;
            TimeSpan startValue;
            TimeSpan endValue;
            if (dutyString == "10:00 AM - 7:00 PM")
            {
                startValue = new TimeSpan (10, 0, 0);
                endValue = new TimeSpan (19, 0, 0);
                data.StartHour = "10:00";
                data.EndHour = "19:00";
            }
            else if (dutyString == "08:00 AM - 5:00 PM")
            {
                startValue = new TimeSpan(8, 0, 0);
                endValue = new TimeSpan(17, 0, 0);
                data.StartHour = "08:00";
                data.EndHour = "17:00";
            }
            else
            {
                startValue = new TimeSpan(12, 0, 0);
                endValue = new TimeSpan(21, 0, 0);
                data.StartHour = "12:00";
                data.EndHour = "21:00";
            }
            data.WorkDays.ForEach(x =>
            {
                x.WorkStartHour = x.WorkStartHour.HasValue ? x.WorkStartHour.Value.Date.Add(startValue) : x.WorkStartHour.Value;
                x.WorkEndHour = x.WorkEndHour.HasValue ? x.WorkEndHour.Value.Date.Add(endValue) : x.WorkEndHour.Value;
            });
        }

        [HttpPost]
        public void UpdateWaitingListData([FromBody] string[] activeIds)
        {
            if (activeIds != null && activeIds.Count() > 0)
            {
                foreach (string ID in activeIds)
                {
                    service.WaitingLists = service.WaitingLists.Where(item => !ID.Contains(item.Id.ToString())).ToList();
                    ViewBag.WaitingList = service.WaitingLists;
                }
            }
        }

        [HttpPost]
        public void UpdateActivityData([FromBody] Activity activityData)
        {
            if (activityData != null)
            {
                activityData.ActivityTime = DateTime.Now; 
                service.Activities.Insert(0, activityData);
            }
        }

        [HttpPost]
        public void UpdateHospitalData([FromBody] EditParams param)
        {
            if(param.added != null && param.added.Count() > 0)
            {
                foreach (Hospital item in param.added)
                {
                    DateTime startTime = Convert.ToDateTime(item.StartTime);
                    DateTime endTime = Convert.ToDateTime(item.EndTime);

                    Hospital hospital = new()
                    {
                        Id = item.Id,
                        Name = item.Name,
                        StartTime = startTime,
                        EndTime = endTime,
                        Disease = item.Disease,
                        DepartmentName = item.DepartmentName,
                        DepartmentId = item.DepartmentId,
                        DoctorId = item.DoctorId,
                        PatientId = item.PatientId,
                        RecurrenceRule = item.RecurrenceRule,
                        Symptoms = item.Symptoms,
                        IsAllDay = item.IsAllDay,
                        ElementType = item.ElementType,
                        IsBlock = item.IsBlock,
                        RecurrenceID = item.RecurrenceID,
                        RecurrenceException = item.RecurrenceException
                    };
                    service.Hospitals.Add(hospital);
                }
            }
            if (param.changed != null && param.changed.Count() > 0)
            {
                foreach (Hospital item in param.changed)
                {
                    DateTime startTime = Convert.ToDateTime(item.StartTime);
                    DateTime endTime = Convert.ToDateTime(item.EndTime);

                    Hospital hospital = service.Hospitals.Single(hospitalItem => hospitalItem.Id == item.Id);

                    hospital.Name = item.Name;
                    hospital.StartTime = startTime;
                    hospital.EndTime = endTime;
                    hospital.Disease = item.Disease;
                    hospital.DepartmentName = item.DepartmentName;
                    hospital.DepartmentId = item.DepartmentId;
                    hospital.DoctorId = item.DoctorId;
                    hospital.PatientId = item.PatientId;
                    hospital.RecurrenceRule = item.RecurrenceRule;
                    hospital.Symptoms = item.Symptoms;
                    hospital.IsAllDay = item.IsAllDay;
                    hospital.ElementType = item.ElementType;
                    hospital.IsBlock = item.IsBlock;
                    hospital.RecurrenceID = item.RecurrenceID;
                    hospital.RecurrenceException = item.RecurrenceException;
                }
            }
            if (param.deleted != null && param.deleted.Count() > 0)
            {
                foreach (Hospital item in param.deleted)
                {
                    Hospital hospital = service.Hospitals.Single(hospitalItem => hospitalItem.Id == item.Id);
                    service.Hospitals.Remove(hospital);
                }
            }
        }

        [HttpPost]
        public void UpdatePatients([FromBody] Params param)
        {
            if (!string.IsNullOrEmpty(param.Value))
            {
                service.Patients = service.Patients.Where(item => item.Id.ToString() != param.Value.ToString()).ToList();
            }
        }

        [HttpPost]
        public IActionResult UpdatePatientData([FromBody] Patient patient)
        {
            if (patient != null)
            {
                string dialogState = string.Empty;
                if (patient.Id == 0)
                {
                    dialogState = "new";
                    patient.Id = service.Patients.Max(item => item.Id) + 1;
                    service.Patients.Add(patient);
                }
                else
                {
                    service.ActivePatients = service.Patients.First(x => x.Id == patient.Id);
                    service.ActivePatients.Name = patient.Name;
                    service.ActivePatients.Gender = patient.Gender;
                    service.ActivePatients.DOB = patient.DOB;
                    service.ActivePatients.BloodGroup = patient.BloodGroup;
                    service.ActivePatients.Mobile = patient.Mobile;
                    service.ActivePatients.Email = patient.Email;
                    service.ActivePatients.Symptoms = patient.Symptoms;
                    patient = service.ActivePatients;
                }
                Activity activity = new()
                {
                    Name = dialogState == "new" ? "Added New Patient" : "Updated Patient",
                    Message = patient.Name + " for " + patient.Symptoms,
                    Time = "10 mins ago",
                    Type = "patient",
                    ActivityTime = DateTime.Now
                };
                service.Activities.Insert(0, activity);
            }
            return Ok(patient);
        }

        [HttpPost]
        public IActionResult ResetService()
        {
            var plannerService = new AppointmentService();
            service.Activities = plannerService.Activities;
            service.StartDate = plannerService.StartDate;
            service.ActiveDoctors = plannerService.ActiveDoctors;
            service.ActivePatients = plannerService.ActivePatients;
            service.StartHours = plannerService.StartHours;
            service.EndHours = plannerService.EndHours;
            service.Views = plannerService.Views;
            service.ColorCategory = plannerService.ColorCategory;
            service.BloodGroups = plannerService.BloodGroups;
            service.DayOfWeekList = plannerService.DayOfWeekList;
            service.TimeSlot = plannerService.TimeSlot;
            service.Hospitals = plannerService.Hospitals;
            service.Patients = plannerService.Patients;
            service.Doctors = plannerService.Doctors;
            service.FilteredDoctors = plannerService.FilteredDoctors;
            service.WaitingLists = plannerService.WaitingLists;
            service.Specializations = plannerService.Specializations;
            service.DutyTimings = plannerService.DutyTimings;
            service.Experience = plannerService.Experience;
            service.CalendarSettings = plannerService.CalendarSettings;
            return Ok();
        }
    }
}