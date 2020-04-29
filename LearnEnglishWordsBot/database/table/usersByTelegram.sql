CREATE TABLE public.usersbytelegram
(
    id SERIAL PRIMARY KEY,
    idchat integer,
    iduser integer,    
    FOREIGN KEY (iduser) REFERENCES users (id) ON UPDATE CASCADE ON DELETE NO ACTION
)