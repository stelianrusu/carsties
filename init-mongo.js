db.createUser(
    {
        user: "stelian",
        pwd: "stelianpw",
        roles: [
            {
                role: "readWrite",
                db: "steliandb"
            }
        ]
    }
);
db.createCollection("test"); //MongoDB creates the database when you first store data in that database