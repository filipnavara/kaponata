# Guacamole is built using maven:3-jdk-8, so let's use the same JDK version for building
# the guacamole-auth-rest module, too:
# https://github.com/apache/guacamole-client/blob/master/Dockerfile#L31
FROM maven:3.6.3-jdk-8 AS build

ENV GUACAMOLE_AUTH_REST_VERSION=896baa1038ffaa745a19a24f8759e449a471f8ad

# Clone the guacamole-auth-rest module, and checkout out a specific commit.
RUN git clone https://github.com/soulwing/guacamole-auth-rest \
&& cd guacamole-auth-rest \
&& git checkout ${GUACAMOLE_AUTH_REST_VERSION}

WORKDIR /guacamole-auth-rest
RUN mvn package

FROM guacamole/guacamole:1.3.0

ENV GUACAMOLE_HOME /quamotion/.guacamole

COPY guacamole.properties $GUACAMOLE_HOME/
COPY --from=build /guacamole-auth-rest/target/dependency/*.jar $GUACAMOLE_HOME/lib/
COPY --from=build /guacamole-auth-rest/target/guacamole-auth-rest-1.0.0-SNAPSHOT.jar $GUACAMOLE_HOME/extensions/
