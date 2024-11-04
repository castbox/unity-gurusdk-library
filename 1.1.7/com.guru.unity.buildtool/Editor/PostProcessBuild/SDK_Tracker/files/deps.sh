#!/bin/bash

source .deps_env

export GURU_BIN="~/dev/flutter/guru_app/tools/bin"
export PATH="$GURU_BIN:$PATH"

echo "--- BuildName: ${BUILD_NAME}"
echo "--- AppName: ${APP_NAME}"
echo "--- APP_ID: ${APP_ID}"
echo "--- Platform: ${PLATFORM}"
echo "--- DIR: ${DIR}"
echo "--- GRADLE_HOME: ${GRADLE_HOME}"

if [ "${PLATFORM}" = "android" ]; then
    # GRADLE_HOME will export by DepsOutputHelper.cs with unity embedded gradle_path
    export PATH="${GRADLE_HOME}:$PATH"
    export PATH="${GRADLE_HOME}/bin:$PATH"
  
    gradle w
fi

echo "----- collect deps start-----"

depaudit --platform "${PLATFORM}" --dir "${DIR}" --build_version "${BUILD_NAME}" --app_name "${APP_NAME}" --app_id "${APP_ID}" --engine unity

echo "----- collect deps over -----"

if [ "${PLATFORM}" = "android" ]; then
  
  if [ -f ${DIR}/gradlew ]; then
      ${DIR}/gradlew --stop
      echo "***************** deps collect success! *****************"
  else
      echo "***************** gradlew not found, deps collect failed! *****************"
  fi
  
else
  echo "***************** deps collect success! *****************"
fi