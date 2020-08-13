import cats.effect.{IO, Resource}

package object biai {

  implicit class IOExtensions[A](io: IO[A]) {
    def asResource: Resource[IO, A] = Resource.liftF(io)
  }

}
