using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using quanlykhoupdate.common;
using quanlykhoupdate.Models;
using quanlykhoupdate.ViewModel;

namespace quanlykhoupdate.Service
{
    public class UserTokenService : IUserTokenService
    {
        private readonly DBContext _context;
        public UserTokenService(DBContext context)
        {
            _context = context;
        }
        public async Task<PayLoad<UserToKenAppDTO>> Add(UserToKenAppDTO add)
        {
            var checkToken = _context.usetokenapp.FirstOrDefault(x => x.token == add.token);
            if(checkToken == null)
            {
                _context.usetokenapp.Add(new usetokenapp
                {
                    token = add.token
                });

                _context.SaveChanges();
            }
            
            return await Task.FromResult(PayLoad<UserToKenAppDTO>.Successfully(add));
        }

        public async Task<PayLoad<string>> SendNotify()
        {
            try { 
            //Cách 1
                var data = _context.usetokenapp.ToList();
                if (data.Count <= 0)
                    return await Task.FromResult(PayLoad<string>.CreatedFail(Status.DATANULL));

                var checkList = new List<string>();
                foreach (var item in data)
                {
                    if (!checkList.Contains(item.token))
                    {
                        try
                        {
                            var messageSend = new Message
                            {
                                Token = item.token,
                                Notification = new Notification()
                                {
                                    Title = "💫🕳💫🕳🕳💯 New Plan Notification",
                                    Body = "There is a plan from Admin just created"
                                }
                            };
                            await FirebaseMessaging.DefaultInstance.SendAsync(messageSend);


                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                        checkList.Add(item.token);
                    }

                }

                return await Task.FromResult(PayLoad<string>.Successfully(Status.SUCCESS));

            }
            catch(Exception ex)
            {
                return await Task.FromResult(PayLoad<string>.CreatedFail(ex.Message));
            }
        }
    }
}
