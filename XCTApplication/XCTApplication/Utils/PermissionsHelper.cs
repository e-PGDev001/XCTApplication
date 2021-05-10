using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;

namespace XCTApplication.Utils
{
    public static class PermissionsHelper
    {
        public static async Task<bool> NewCheckPermission(Permission permission)
        {
            PermissionStatus pms;
            switch (permission)
            {
                case Permission.Calendar:
                    {
                        pms = await CrossPermissions.Current.CheckPermissionStatusAsync<CalendarPermission>();
                    }
                    break;

                case Permission.Camera:
                    {
                        pms = await CrossPermissions.Current.CheckPermissionStatusAsync<CameraPermission>();
                    }
                    break;

                case Permission.Contacts:
                    {
                        pms = await CrossPermissions.Current.CheckPermissionStatusAsync<ContactsPermission>();
                    }
                    break;

                case Permission.Location:
                    {
                        pms = await CrossPermissions.Current.CheckPermissionStatusAsync<LocationPermission>();
                    }
                    break;

                case Permission.LocationWhenInUse:
                    {
                        pms = await CrossPermissions.Current.CheckPermissionStatusAsync<LocationWhenInUsePermission>();
                    }
                    break;

                case Permission.LocationAlways:
                    {
                        pms = await CrossPermissions.Current.CheckPermissionStatusAsync<LocationAlwaysPermission>();
                    }
                    break;

                case Permission.MediaLibrary:
                    {
                        pms = await CrossPermissions.Current.CheckPermissionStatusAsync<MediaLibraryPermission>();
                    }
                    break;

                case Permission.Microphone:
                    {
                        pms = await CrossPermissions.Current.CheckPermissionStatusAsync<MicrophonePermission>();
                    }
                    break;

                case Permission.Phone:
                    {
                        pms = await CrossPermissions.Current.CheckPermissionStatusAsync<PhonePermission>();
                    }
                    break;

                case Permission.Photos:
                    {
                        pms = await CrossPermissions.Current.CheckPermissionStatusAsync<PhotosPermission>();
                    }
                    break;

                case Permission.Reminders:
                    {
                        pms = await CrossPermissions.Current.CheckPermissionStatusAsync<RemindersPermission>();
                    }
                    break;

                case Permission.Sensors:
                    {
                        pms = await CrossPermissions.Current.CheckPermissionStatusAsync<SensorsPermission>();
                    }
                    break;

                case Permission.Sms:
                    {
                        pms = await CrossPermissions.Current.CheckPermissionStatusAsync<SmsPermission>();
                    }
                    break;

                case Permission.Speech:
                    {
                        pms = await CrossPermissions.Current.CheckPermissionStatusAsync<SpeechPermission>();
                    }
                    break;

                case Permission.Storage:
                    {
                        pms = await CrossPermissions.Current.CheckPermissionStatusAsync<StoragePermission>();
                    }
                    break;

                default:
                    {
                        pms = await CrossPermissions.Current.CheckPermissionStatusAsync<StoragePermission>();
                    }
                    break;
            }

            if (pms != PermissionStatus.Granted)
            {
                PermissionStatus newPms;
                switch (permission)
                {
                    case Permission.Calendar:
                        {
                            newPms = await CrossPermissions.Current.RequestPermissionAsync<CalendarPermission>();
                        }
                        break;

                    case Permission.Camera:
                        {
                            newPms = await CrossPermissions.Current.RequestPermissionAsync<CameraPermission>();
                        }
                        break;

                    case Permission.Contacts:
                        {
                            newPms = await CrossPermissions.Current.RequestPermissionAsync<ContactsPermission>();
                        }
                        break;

                    case Permission.Location:
                        {
                            newPms = await CrossPermissions.Current.RequestPermissionAsync<LocationPermission>();
                        }
                        break;

                    case Permission.LocationWhenInUse:
                        {
                            newPms =
                                await CrossPermissions.Current.RequestPermissionAsync<LocationWhenInUsePermission>();
                        }
                        break;

                    case Permission.LocationAlways:
                        {
                            newPms = await CrossPermissions.Current.RequestPermissionAsync<LocationAlwaysPermission>();
                        }
                        break;

                    case Permission.MediaLibrary:
                        {
                            newPms = await CrossPermissions.Current.RequestPermissionAsync<MediaLibraryPermission>();
                        }
                        break;

                    case Permission.Microphone:
                        {
                            newPms = await CrossPermissions.Current.RequestPermissionAsync<MicrophonePermission>();
                        }
                        break;

                    case Permission.Phone:
                        {
                            newPms = await CrossPermissions.Current.RequestPermissionAsync<PhonePermission>();
                        }
                        break;

                    case Permission.Photos:
                        {
                            newPms = await CrossPermissions.Current.RequestPermissionAsync<PhotosPermission>();
                        }
                        break;

                    case Permission.Reminders:
                        {
                            newPms = await CrossPermissions.Current.RequestPermissionAsync<RemindersPermission>();
                        }
                        break;

                    case Permission.Sensors:
                        {
                            newPms = await CrossPermissions.Current.RequestPermissionAsync<SensorsPermission>();
                        }
                        break;

                    case Permission.Sms:
                        {
                            newPms = await CrossPermissions.Current.RequestPermissionAsync<SmsPermission>();
                        }
                        break;

                    case Permission.Speech:
                        {
                            newPms = await CrossPermissions.Current.RequestPermissionAsync<SpeechPermission>();
                        }
                        break;

                    case Permission.Storage:
                        {
                            newPms = await CrossPermissions.Current.RequestPermissionAsync<StoragePermission>();
                        }
                        break;

                    default:
                        {
                            newPms = await CrossPermissions.Current.RequestPermissionAsync<StoragePermission>();
                        }
                        break;
                }

                return newPms == PermissionStatus.Granted;
            }

            // small delay
            await Task.Delay(TimeSpan.FromMilliseconds(100));

            return true;
        }
    }

    public static class XFViewExtensions
    {
        public static T As<T>(this object instance, string context = null)
        {
            Debug.WriteLine($"Call from: {context}");

            if (instance == null)
#if DEBUG || DB_SMH
                throw new System.ArgumentNullException(nameof(instance),
                    $"Unable to sure cast null instance as type '{typeof(T).Name}");
#else
                return default(T);
#endif
            return (T)instance;
        }
    }
}
