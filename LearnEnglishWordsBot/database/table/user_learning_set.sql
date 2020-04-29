CREATE TABLE public.user_learning_set
(
    id SERIAL PRIMARY KEY,
    id_user int,
    id_learn_set int,
    FOREIGN KEY (id_user) REFERENCES users (id) ON UPDATE CASCADE ON DELETE NO ACTION,
    FOREIGN KEY (id_learn_set) REFERENCES learn_set (id) ON UPDATE CASCADE ON DELETE NO ACTION
);

CREATE UNIQUE INDEX user_learning_set_id_user_and_id_learn_set ON user_learning_set (id_user, id_learn_set);