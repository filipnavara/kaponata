// <copyright file="KubernetesObjectExtensions.cs" company="Quamotion bv">
// Copyright (c) Quamotion bv. All rights reserved.
// </copyright>

using k8s;
using k8s.Models;
using Microsoft.Rest;

namespace Kaponata.Kubernetes
{
    /// <summary>
    /// Provides extension methods for the <see cref="IKubernetesObject{TMetadata}"/> interface.
    /// </summary>
    public static class KubernetesObjectExtensions
    {
        /// <summary>
        /// Creates a <see cref="V1OwnerReference"/> which references this <see cref="IKubernetesObject{TMetadata}"/>.
        /// </summary>
        /// <param name="value">
        /// The object for which to create an owner reference.
        /// </param>
        /// <param name="blockOwnerDeletion">
        /// Gets get or sets a value indicating whether the owner cannot be deleted from the
        /// clsuter until reference is removed.
        /// </param>
        /// <param name="controller">
        /// Gets or sets a value indicating whether this reference points to the managing controller.
        /// </param>
        /// <returns>
        /// The owner reference for the object.
        /// </returns>
        public static V1OwnerReference AsOwnerReference(
            this IKubernetesObject<V1ObjectMeta> value,
            bool? blockOwnerDeletion = null,
            bool? controller = null)
        {
            if (value.ApiVersion == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "value.ApiVersion");
            }

            if (value.Kind == null)
            {
                throw new ValidationException(ValidationRules.CannotBeNull, "value.Kind");
            }

            // To be kept in sync with https://github.com/kubernetes-client/csharp/blob/5be3cff425b91b1d16dd0361bf7756a0cb779d8d/src/KubernetesClient/ModelExtensions.cs#L610
            return new V1OwnerReference()
            {
                ApiVersion = value.ApiVersion,
                Kind = value.Kind,
                Name = value.Name(),
                Uid = value.Uid(),
                BlockOwnerDeletion = blockOwnerDeletion,
                Controller = controller,
            };
        }
    }
}
