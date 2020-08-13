import mill._, scalalib._

object biai extends ScalaModule {
    def scalaVersion = "2.13.3"

    val http4sVersion = "0.21.6"

    def ivyDeps = Agg(
        ivy"org.typelevel::cats-effect:2.1.3",
        ivy"io.methvin::directory-watcher-better-files:0.9.10",
        ivy"com.github.pureconfig::pureconfig:0.13.0",
        ivy"com.github.pureconfig::pureconfig-cats-effect:0.13.0",
        ivy"org.http4s::http4s-dsl:$http4sVersion",
        ivy"org.http4s::http4s-blaze-client:$http4sVersion",
        ivy"org.http4s::http4s-circe:$http4sVersion",
        ivy"io.circe::circe-generic:0.12.3",
        ivy"co.fs2::fs2-core:2.4.0",
        //ivy"ch.qos.logback:logback-classic:1.2.3"
    )
}
