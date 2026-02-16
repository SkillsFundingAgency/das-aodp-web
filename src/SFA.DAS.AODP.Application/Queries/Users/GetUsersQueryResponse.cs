public class GetUsersQueryResponse
{
    public List<User> Users { get; set; }

    public class User
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }
    }
}